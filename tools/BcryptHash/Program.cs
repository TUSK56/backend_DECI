var pwd = args.Length > 0 ? args[0] : "Deci123!";
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(pwd));
