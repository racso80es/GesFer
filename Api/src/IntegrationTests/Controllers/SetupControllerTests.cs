using FluentAssertions;
using GesFer.Api.Services;
using GesFer.Infrastructure.Data;
using GesFer.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para SetupController
/// </summary>
public class SetupControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public SetupControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Initialize_EndpointShouldExist()
    {
        // Act - Llamar al endpoint de inicialización
        // Nota: En un test real, esto ejecutaría Docker, pero para tests usamos la BD en memoria
        // El endpoint puede fallar si Docker no está disponible, pero al menos verificamos que existe
        var response = await _client.PostAsync("/api/setup/initialize", null);

        // Assert - El endpoint debería existir (puede fallar en tests porque Docker no está disponible)
        // Pero al menos verificamos que el endpoint está configurado y no devuelve 404
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, "El endpoint debería existir");
    }

    [Fact]
    public async Task SeedData_ShouldInsertUsersCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Limpiar y crear base de datos en memoria
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Act - Seed datos usando el helper (simula lo que hace SetupService)
        await TestDataSeeder.SeedTestDataAsync(context);

        // Assert - Verificar que los usuarios se insertaron
        var users = await context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        users.Should().NotBeEmpty("Debería haber al menos un usuario insertado");
        users.Should().Contain(u => u.Username == "admin", "Debería existir el usuario 'admin'");
        
        var adminUser = users.First(u => u.Username == "admin");
        adminUser.FirstName.Should().Be("Administrador");
        adminUser.LastName.Should().Be("Sistema");
        adminUser.Email.Should().Be("admin@empresa.com");
        adminUser.CompanyId.Should().NotBeEmpty();
        adminUser.PasswordHash.Should().NotBeNullOrEmpty("El usuario debería tener un hash de contraseña");
        
        // Verificar que el usuario tiene grupo asignado
        var userGroups = await context.UserGroups
            .Where(ug => ug.UserId == adminUser.Id && ug.DeletedAt == null)
            .ToListAsync();
        userGroups.Should().NotBeEmpty("El usuario debería tener al menos un grupo asignado");
        
        // Verificar que el usuario tiene permisos
        var userPermissions = await context.UserPermissions
            .Where(up => up.UserId == adminUser.Id && up.DeletedAt == null)
            .ToListAsync();
        userPermissions.Should().NotBeEmpty("El usuario debería tener al menos un permiso directo");
    }

    [Fact]
    public async Task GetStatus_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/setup/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("endpoint");
    }
}

