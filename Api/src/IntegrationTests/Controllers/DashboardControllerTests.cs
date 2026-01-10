using FluentAssertions;
using GesFer.Application.DTOs.Admin;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using GesFer.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para DashboardController
/// Valida autorización con rol Admin, creación de AuditLog y uso de Sequential GUIDs
/// </summary>
public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public DashboardControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Asegurar que la base de datos esté creada
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed datos (incluye AdminUser de prueba)
        await TestDataSeeder.SeedTestDataAsync(context);
    }

    private string GenerateAdminToken(string cursorId, string username, string userId)
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var secretKey = configuration["JwtSettings:SecretKey"] 
            ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-hs256-algorithm";
        var issuer = configuration["JwtSettings:Issuer"] ?? "GesFer";
        var audience = configuration["JwtSettings:Audience"] ?? "GesFer";

        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, cursorId),
            new Claim(ClaimTypes.Name, username),
            new Claim("UserId", userId),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GetSummary_WithValidAdminToken_ShouldReturnDashboardSummary()
    {
        // Arrange
        var adminUser = await GetAdminUserAsync();
        var cursorId = adminUser.Id.ToString();
        var token = GenerateAdminToken(cursorId, adminUser.Username, adminUser.Id.ToString());
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/admin/dashboard/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            $"El endpoint debería devolver 200 OK, pero devolvió {response.StatusCode}. " +
            $"Respuesta: {await response.Content.ReadAsStringAsync()}");
        
        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        summary.Should().NotBeNull("La respuesta no debería ser null");
        summary!.TotalCompanies.Should().BeGreaterOrEqualTo(0);
        summary.TotalUsers.Should().BeGreaterOrEqualTo(0);
        summary.ActiveUsers.Should().BeGreaterOrEqualTo(0);
        summary.TotalArticles.Should().BeGreaterOrEqualTo(0);
        summary.TotalSuppliers.Should().BeGreaterOrEqualTo(0);
        summary.TotalCustomers.Should().BeGreaterOrEqualTo(0);
        summary.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetSummary_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/admin/dashboard/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_ShouldCreateAuditLog()
    {
        // Arrange
        var adminUser = await GetAdminUserAsync();
        var cursorId = adminUser.Id.ToString();
        var token = GenerateAdminToken(cursorId, adminUser.Username, adminUser.Id.ToString());
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Limpiar logs existentes
        var existingLogs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        context.AuditLogs.RemoveRange(existingLogs);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/dashboard/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que se creó un AuditLog
        var auditLogs = await context.AuditLogs
            .Where(a => a.CursorId == cursorId && a.Action == "GetDashboardSummary")
            .ToListAsync();
        
        auditLogs.Should().HaveCount(1, "Debe crearse exactamente un AuditLog por petición");
        var auditLog = auditLogs.First();
        auditLog.CursorId.Should().Be(cursorId);
        auditLog.Username.Should().Be(adminUser.Username);
        auditLog.Action.Should().Be("GetDashboardSummary");
        auditLog.HttpMethod.Should().Be("GET");
        auditLog.Path.Should().Be("/api/admin/dashboard/summary");
    }

    [Fact]
    public async Task GetSummary_ShouldUseSequentialGuidsForAuditLog()
    {
        // Arrange
        var adminUser = await GetAdminUserAsync();
        var cursorId = adminUser.Id.ToString();
        var token = GenerateAdminToken(cursorId, adminUser.Username, adminUser.Id.ToString());
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Limpiar logs existentes
        var existingLogs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        context.AuditLogs.RemoveRange(existingLogs);
        await context.SaveChangesAsync();

        // Act - Hacer dos peticiones para verificar que los GUIDs son secuenciales
        await _client.GetAsync("/api/admin/dashboard/summary");
        await Task.Delay(10); // Pequeño delay para asegurar diferencia de timestamp
        await _client.GetAsync("/api/admin/dashboard/summary");

        // Assert - Verificar que los AuditLogs tienen IDs generados (no nulos)
        var auditLogs = await context.AuditLogs
            .Where(a => a.CursorId == cursorId && a.Action == "GetDashboardSummary")
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
        
        auditLogs.Should().HaveCountGreaterOrEqualTo(2, "Deben crearse al menos 2 AuditLogs");
        
        // Verificar que los IDs son GUIDs válidos (Sequential GUIDs)
        foreach (var log in auditLogs)
        {
            log.Id.Should().NotBeEmpty("El Id del AuditLog debe ser un GUID válido");
            log.Id.Should().NotBe(Guid.Empty, "El Id del AuditLog no debe ser Guid.Empty");
        }
        
        // Los GUIDs secuenciales deben estar ordenados (el segundo debe ser mayor que el primero)
        // Para MySQL big-endian, los bytes más significativos están al inicio
        var firstId = auditLogs[0].Id;
        var secondId = auditLogs[1].Id;
        
        // Comparar los bytes del GUID para verificar orden secuencial (MySQL big-endian)
        var firstBytes = firstId.ToByteArray();
        var secondBytes = secondId.ToByteArray();
        
        // En MySQL big-endian, los primeros bytes son los más significativos
        // Comparar byte por byte desde el inicio
        var comparisonResult = CompareBytesBigEndian(firstBytes, secondBytes);
        comparisonResult.Should().BeLessOrEqualTo(0, 
            "Los Sequential GUIDs deben estar ordenados correctamente (segundo >= primero) para MySQL big-endian");
    }

    [Fact]
    public async Task GetSummary_AuditLogShouldContainCorrectData()
    {
        // Arrange
        var adminUser = await GetAdminUserAsync();
        var cursorId = adminUser.Id.ToString();
        var token = GenerateAdminToken(cursorId, adminUser.Username, adminUser.Id.ToString());
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Limpiar logs existentes
        var existingLogs = await context.AuditLogs.IgnoreQueryFilters().ToListAsync();
        context.AuditLogs.RemoveRange(existingLogs);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/dashboard/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var auditLog = await context.AuditLogs
            .Where(a => a.CursorId == cursorId && a.Action == "GetDashboardSummary")
            .FirstOrDefaultAsync();
        
        auditLog.Should().NotBeNull("Debe existir un AuditLog");
        auditLog!.CursorId.Should().Be(cursorId);
        auditLog.Username.Should().Be(adminUser.Username);
        auditLog.Action.Should().Be("GetDashboardSummary");
        auditLog.HttpMethod.Should().Be("GET");
        auditLog.Path.Should().Be("/api/admin/dashboard/summary");
        auditLog.ActionTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        auditLog.AdditionalData.Should().NotBeNullOrEmpty("AdditionalData debe contener las métricas serializadas");
    }

    private async Task<AdminUser> GetAdminUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var adminUser = await context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == "admin");
        
        adminUser.Should().NotBeNull("Debe existir un AdminUser de prueba");
        return adminUser!;
    }

    /// <summary>
    /// Compara dos arrays de bytes en orden big-endian (MySQL).
    /// Retorna: negativo si first < second, 0 si first == second, positivo si first > second
    /// </summary>
    private int CompareBytesBigEndian(byte[] first, byte[] second)
    {
        if (first.Length != second.Length)
            return first.Length.CompareTo(second.Length);

        for (int i = 0; i < first.Length; i++)
        {
            var comparison = first[i].CompareTo(second[i]);
            if (comparison != 0)
                return comparison;
        }

        return 0;
    }
}
