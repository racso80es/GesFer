using FluentAssertions;
using GesFer.Domain.ValueObjects;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GesFer.IntegrationTests.Services;

/// <summary>
/// Tests de integración para validar que los Value Objects (Email, TaxId) rechazan datos inválidos
/// y que estos datos inválidos NUNCA llegan a la base de datos durante el seeding.
/// </summary>
[Collection("DatabaseStep")]
public class ValueObjectValidationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ValueObjectValidationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Test que confirma que un TaxId inválido NO llega jamás a la base de datos durante el seeding.
    /// Los datos inválidos están en test-data.json pero deben ser ignorados por el seeder.
    /// </summary>
    [Fact]
    public async Task SeedCustomers_WithInvalidTaxId_ShouldNotPersistToDatabase()
    {
        // Arrange
        using var scope = _fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act: Consultar todos los customers en la base de datos
        var allCustomers = await context.Customers
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert: Verificar que NO existe ningún customer con TaxId inválido
        // Los IDs de customers con datos inválidos en test-data.json son:
        // - ffffffff-0000-0000-0000-000000000001 (TaxId: "INVALID123")
        // - ffffffff-0000-0000-0000-000000000002 (Email inválido)
        // - ffffffff-0000-0000-0000-000000000003 (TaxId y Email inválidos)

        var invalidCustomerIds = new[]
        {
            Guid.Parse("ffffffff-0000-0000-0000-000000000001"), // TaxId inválido
            Guid.Parse("ffffffff-0000-0000-0000-000000000002"), // Email inválido
            Guid.Parse("ffffffff-0000-0000-0000-000000000003")  // Ambos inválidos
        };

        foreach (var invalidId in invalidCustomerIds)
        {
            var invalidCustomer = allCustomers.FirstOrDefault(c => c.Id == invalidId);
            invalidCustomer.Should().BeNull(
                $"El customer con ID {invalidId} tiene datos inválidos y NO debería estar en la base de datos. " +
                $"El seeder debe haberlo ignorado por Violación de Dominio.");
        }

        // Verificar que los customers válidos SÍ están en la base de datos
        var validCustomerIds = new[]
        {
            Guid.Parse("cccccccc-1111-1111-1111-111111111111"), // Cliente Test 1 (válido)
            Guid.Parse("dddddddd-2222-2222-2222-222222222222"), // Cliente Test 2 (válido)
            Guid.Parse("eeeeeeee-3333-3333-3333-333333333333")  // Distribuidora Málaga (válido)
        };

        foreach (var validId in validCustomerIds)
        {
            var validCustomer = allCustomers.FirstOrDefault(c => c.Id == validId);
            validCustomer.Should().NotBeNull(
                $"El customer con ID {validId} tiene datos válidos y DEBERÍA estar en la base de datos.");
        }
    }

    /// <summary>
    /// Test que confirma que un Email inválido NO llega jamás a la base de datos durante el seeding.
    /// </summary>
    [Fact]
    public async Task SeedCustomers_WithInvalidEmail_ShouldNotPersistToDatabase()
    {
        // Arrange
        using var scope = _fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act: Consultar todos los customers en la base de datos
        var allCustomers = await context.Customers
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert: Verificar que NO existe ningún customer con Email inválido
        var invalidEmailCustomerId = Guid.Parse("ffffffff-0000-0000-0000-000000000002");
        var invalidEmailCustomer = allCustomers.FirstOrDefault(c => c.Id == invalidEmailCustomerId);
        
        invalidEmailCustomer.Should().BeNull(
            $"El customer con ID {invalidEmailCustomerId} tiene Email inválido y NO debería estar en la base de datos.");
    }

    /// <summary>
    /// Test que confirma que Companies con TaxId inválido NO llegan a la base de datos durante el seeding.
    /// </summary>
    [Fact]
    public async Task SeedCompanies_WithInvalidTaxId_ShouldNotPersistToDatabase()
    {
        // Arrange
        using var scope = _fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act: Consultar todas las companies en la base de datos
        var allCompanies = await context.Companies
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert: Verificar que NO existe ninguna company con TaxId inválido
        // Los IDs de companies con datos inválidos en test-data.json son:
        // - 11111111-1111-1111-1111-111111111113 (TaxId: "INVALID999")
        // - 11111111-1111-1111-1111-111111111114 (Email inválido)

        var invalidCompanyIds = new[]
        {
            Guid.Parse("11111111-1111-1111-1111-111111111113"), // TaxId inválido
            Guid.Parse("11111111-1111-1111-1111-111111111114")  // Email inválido
        };

        foreach (var invalidId in invalidCompanyIds)
        {
            var invalidCompany = allCompanies.FirstOrDefault(c => c.Id == invalidId);
            invalidCompany.Should().BeNull(
                $"La company con ID {invalidId} tiene datos inválidos y NO debería estar en la base de datos. " +
                $"El seeder debe haberla ignorado por Violación de Dominio.");
        }

        // Verificar que las companies válidas SÍ están en la base de datos
        var validCompanyIds = new[]
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"), // Empresa Demo (válida)
            Guid.Parse("11111111-1111-1111-1111-111111111112")  // Empresa Test Update (válida)
        };

        foreach (var validId in validCompanyIds)
        {
            var validCompany = allCompanies.FirstOrDefault(c => c.Id == validId);
            validCompany.Should().NotBeNull(
                $"La company con ID {validId} tiene datos válidos y DEBERÍA estar en la base de datos.");
        }
    }

    /// <summary>
    /// Test que confirma que el sistema sobrevive a datos corruptos (legacy 'B12345678').
    /// Este test valida la cuarentena de datos inválidos durante el seeding.
    /// El legacy 'B12345678' está en test-data.json pero debe ser rechazado por violación de dominio.
    /// </summary>
    [Fact]
    public async Task SeedCompanies_WithLegacyInvalidTaxId_ShouldSurviveAndNotPersistToDatabase()
    {
        // Arrange
        using var scope = _fixture.Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act: Consultar todas las companies en la base de datos
        var allCompanies = await context.Companies
            .IgnoreQueryFilters()
            .ToListAsync();

        // Assert: Verificar que el legacy 'B12345678' NO está en la base de datos
        // El legacy 'B12345678' está en test-data.json (ID: 11111111-1111-1111-1111-111111111111)
        // pero debe ser rechazado por el algoritmo oficial de validación de CIF
        var legacyCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var legacyCompany = allCompanies.FirstOrDefault(c => c.Id == legacyCompanyId);

        // Verificar que la empresa NO está en la BD (fue rechazada por TaxId inválido)
        legacyCompany.Should().BeNull(
            $"La company con ID {legacyCompanyId} tiene el legacy TaxId 'B12345678' que NO es válido según el algoritmo oficial. " +
            $"El seeder debe haberla rechazado por Violación de Dominio y NO debería estar en la base de datos.");

        // Verificar que el sistema sobrevivió y que otras empresas válidas SÍ están en la BD
        var validCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111112"); // Empresa Test Update (válida)
        var validCompany = allCompanies.FirstOrDefault(c => c.Id == validCompanyId);
        
        validCompany.Should().NotBeNull(
            $"El sistema debe sobrevivir a datos corruptos. La empresa válida con ID {validCompanyId} DEBERÍA estar en la base de datos.");
        
        // Verificar que el TaxId de la empresa válida es correcto
        validCompany!.TaxId.HasValue.Should().BeTrue("La empresa válida debe tener un TaxId");
        validCompany.TaxId!.Value.Value.Should().Be("B87654321", "El TaxId debe ser el valor válido esperado");
    }

    /// <summary>
    /// Test que confirma que los Value Objects rechazan correctamente datos inválidos.
    /// Este test valida la lógica de validación de los Value Objects directamente.
    /// </summary>
    [Fact]
    public void TaxId_Create_WithInvalidData_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidTaxIds = new[]
        {
            "INVALID123",      // Formato completamente inválido
            "BADTAXID",        // Sin formato CIF/NIF/NIE
            "12345678",        // Solo números, falta letra
            "A1234567",        // Formato incorrecto
            ""                 // Vacío
        };

        // Act & Assert
        foreach (var invalidTaxId in invalidTaxIds)
        {
            if (string.IsNullOrWhiteSpace(invalidTaxId))
            {
                // TaxId vacío debe lanzar ArgumentException
                Action act = () => TaxId.Create(invalidTaxId);
                act.Should().Throw<ArgumentException>()
                    .WithMessage("*no puede ser nulo o vacío*");
            }
            else
            {
                // TaxId con formato inválido debe lanzar ArgumentException
                Action act = () => TaxId.Create(invalidTaxId);
                act.Should().Throw<ArgumentException>()
                    .WithMessage("*no es válido*");
            }
        }
    }

    /// <summary>
    /// Test que confirma que Email rechaza correctamente datos inválidos.
    /// </summary>
    [Fact]
    public void Email_Create_WithInvalidData_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEmails = new[]
        {
            "email-sin-arroba-invalido",  // Sin @
            "email.mal.formato",          // Sin @
            "email@",                     // Sin dominio
            "@dominio.com",               // Sin usuario
            "email @dominio.com",         // Con espacio
            ""                            // Vacío
        };

        // Act & Assert
        foreach (var invalidEmail in invalidEmails)
        {
            if (string.IsNullOrWhiteSpace(invalidEmail))
            {
                // Email vacío debe lanzar ArgumentException
                Action act = () => Email.Create(invalidEmail);
                act.Should().Throw<ArgumentException>()
                    .WithMessage("*no puede ser nulo o vacío*");
            }
            else
            {
                // Email con formato inválido debe lanzar ArgumentException
                Action act = () => Email.Create(invalidEmail);
                act.Should().Throw<ArgumentException>()
                    .WithMessage("*no es válido*");
            }
        }
    }
}
