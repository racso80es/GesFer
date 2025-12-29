using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        var password = "admin123";
        var hashFromSeed = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";
        
        Console.WriteLine($"Verificando contraseña: {password}");
        Console.WriteLine($"Hash del seed-data.sql: {hashFromSeed}");
        
        var isValid = BCrypt.Net.BCrypt.Verify(password, hashFromSeed);
        Console.WriteLine($"¿El hash verifica correctamente? {isValid}");
        
        if (!isValid)
        {
            Console.WriteLine("ERROR: El hash no verifica correctamente. Generando nuevo hash...");
            var newHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
            Console.WriteLine($"Nuevo hash: {newHash}");
        }
        else
        {
            Console.WriteLine("OK: El hash verifica correctamente.");
        }
    }
}
