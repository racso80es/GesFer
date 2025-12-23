using System;
using BCrypt.Net;
class Program { static void Main(){ var hash = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(11)); Console.WriteLine(hash); } }
