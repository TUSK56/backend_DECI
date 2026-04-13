namespace Deci.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string fullName, string role, bool profileCompleted);
}
