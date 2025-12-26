using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        var password = "admin123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
        Console.WriteLine($"Contraseña: {password}");
        Console.WriteLine($"Hash BCrypt: {hash}");
    }
}
