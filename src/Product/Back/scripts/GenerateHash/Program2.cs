using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        var password = "admin123";
        var newHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
        Console.WriteLine($"Nuevo hash: {newHash}");
        var isValid = BCrypt.Net.BCrypt.Verify(password, newHash);
        Console.WriteLine($"Verificado: {isValid}");
    }
}
