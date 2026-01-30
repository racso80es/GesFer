// Script para generar hash de contraseña con BCrypt
// Ejecutar con: dotnet script generate-password-hash.cs

using BCrypt.Net;

var password = "admin123";
var hash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(11));

Console.WriteLine($"Contraseña: {password}");
Console.WriteLine($"Hash BCrypt: {hash}");

