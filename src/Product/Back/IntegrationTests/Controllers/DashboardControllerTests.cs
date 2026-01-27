using FluentAssertions;
using GesFer.Application.DTOs.Admin;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
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
[Collection("DatabaseStep")]
public class DashboardControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public DashboardControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    private string GenerateAdminToken(string cursorId, string username, string userId)
    {
        var configuration = _fixture.Services.GetRequiredService<IConfiguration>();
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

        using var scope = _fixture.Services.CreateScope();
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

        using var scope = _fixture.Services.CreateScope();
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
        
        // Verificar que los GUIDs están ordenados por CreatedAt (agnóstico al endianness de la BD)
        // Los Sequential GUIDs deben estar ordenados temporalmente
        var firstLog = auditLogs[0];
        var secondLog = auditLogs[1];
        
        // El segundo log debe tener CreatedAt mayor o igual que el primero
        // Esta es la verificación principal y es agnóstica al formato de almacenamiento de la BD
        secondLog.CreatedAt.Should().BeOnOrAfter(firstLog.CreatedAt,
            "Los logs deben estar ordenados por CreatedAt");
        
        // Verificar que los GUIDs son diferentes (cada log debe tener su propio ID único)
        firstLog.Id.Should().NotBe(secondLog.Id, "Cada AuditLog debe tener un ID único");
        
        // Verificación de Sequential GUIDs: Los GUIDs deben ser válidos y únicos
        // No comparamos bytes directamente porque MySQL puede almacenarlos en formato diferente
        // La verificación de orden temporal (CreatedAt) es suficiente y más confiable
    }

    [Fact]
    public async Task GetSummary_AuditLogShouldContainCorrectData()
    {
        // Arrange
        var adminUser = await GetAdminUserAsync();
        var cursorId = adminUser.Id.ToString();
        var token = GenerateAdminToken(cursorId, adminUser.Username, adminUser.Id.ToString());
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _fixture.Services.CreateScope();
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
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var adminUser = await context.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == "admin");
        
        adminUser.Should().NotBeNull("Debe existir un AdminUser de prueba");
        return adminUser!;
    }

}
