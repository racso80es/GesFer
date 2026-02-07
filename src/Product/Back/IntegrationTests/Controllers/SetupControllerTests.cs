using FluentAssertions;
using GesFer.Infrastructure.Data;
using GesFer.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para SetupController
/// </summary>
[Collection("DatabaseStep")]
public class SetupControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public SetupControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
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
        // Arrange - La base de datos ya está inicializada por DatabaseFixture
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Assert - Verificar que los usuarios se insertaron (ya están en la BD por el fixture)
        var users = await context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        users.Should().NotBeEmpty("Debería haber al menos un usuario insertado");
        users.Should().Contain(u => u.Username == "admin", "Debería existir el usuario 'admin'");
        
        var adminUser = users.First(u => u.Username == "admin");
        adminUser.FirstName.Should().Be("Administrador");
        adminUser.LastName.Should().Be("Sistema");
        // FIX: Acceder a .Value.Value para comparar con string, ya que Email es ValueObject
        adminUser.Email.HasValue.Should().BeTrue();
        adminUser.Email!.Value.Value.Should().Be("admin@empresa.com");

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
