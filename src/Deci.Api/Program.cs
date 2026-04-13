using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Deci.Application.Interfaces;
using Deci.Api.Middleware;
using Deci.Infrastructure;
using Deci.Infrastructure.Persistence;
using Deci.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// IIS often uses a working directory that is NOT the app folder — always log under BaseDirectory.
var baseDir = AppContext.BaseDirectory;

void Trace(string msg)
{
    try
    {
        File.AppendAllText(Path.Combine(baseDir, "startup-trace.txt"),
            $"{DateTime.UtcNow:O} {msg}{Environment.NewLine}");
    }
    catch
    {
        /* ignore */
    }
}

try
{
    Trace("1 entered try (CLR running)");
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = baseDir,
        WebRootPath = Path.Combine(baseDir, "wwwroot"),
    });

    Trace("2 after CreateBuilder");
    // Optional secrets on the server (gitignored name); overrides appsettings.Production.json
    builder.Configuration.AddJsonFile(
        Path.Combine(baseDir, "appsettings.Production.local.json"),
        optional: true,
        reloadOnChange: false);

    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    if (string.IsNullOrWhiteSpace(conn))
    {
        if (builder.Environment.IsProduction())
            throw new InvalidOperationException(
                "Missing SQL connection string. Add ConnectionStrings:DefaultConnection to appsettings.Production.json on the server, " +
                "or upload appsettings.Production.local.json next to Deci.Api.dll (copy from appsettings.Production.local.json.example), " +
                "or set environment variable ConnectionStrings__DefaultConnection. Do not rely on LocalDB on the server.");
        conn = "Server=(localdb)\\mssqllocaldb;Database=DeciLocal;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    builder.Services.AddPersistence(conn);
    builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
    builder.Services.AddScoped<IJwtService, JwtService>();

    var jwtKey = builder.Configuration["Jwt:Key"] ?? "DECI_DEV_ONLY_CHANGE_THIS_KEY_32_CHARS_MIN!!";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DeciApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DeciPortal",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddCors(o => o.AddPolicy("front", p =>
    {
        var configuredOrigin = builder.Configuration["Frontend:Origin"];
        p.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;
            if (origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(origin, "http://127.0.0.1:5173", StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.IsNullOrWhiteSpace(configuredOrigin) &&
                string.Equals(origin, configuredOrigin.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
                u.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    }));

    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "DECI Management API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header: Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() },
        });
    });

    Trace("3 before Build");
    var app = builder.Build();
    Trace("4 after Build");

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DECI Management API v1"));

    app.UseCors("front");
    Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads"));
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseMiddleware<ProfileCompletionMiddleware>();
    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        try
        {
            await db.Database.MigrateAsync();
            await DbSeeder.SeedAsync(db, hasher);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migrate/seed skipped (check SQL connection).");
        }
    }

    Trace("5 before Run");
    app.Run();
}
catch (Exception ex)
{
    try
    {
        File.WriteAllText(Path.Combine(baseDir, "startup-error.txt"), ex.ToString());
    }
    catch
    {
        /* ignore */
    }

    throw;
}
