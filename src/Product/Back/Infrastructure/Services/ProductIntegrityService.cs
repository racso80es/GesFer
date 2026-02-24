using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Services;
using GesFer.Shared.Back.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de verificación de integridad para Product
/// </summary>
public class ProductIntegrityService : IIntegrityCheckService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductIntegrityService> _logger;
    private readonly ISensitiveDataSanitizer _sanitizer;
    private readonly IHostEnvironment _environment;
    private readonly IServiceProvider _serviceProvider;

    public ProductIntegrityService(
        ProductDbContext context,
        ILogger<ProductIntegrityService> logger,
        ISensitiveDataSanitizer sanitizer,
        IHostEnvironment environment,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _sanitizer = sanitizer;
        _environment = environment;
        _serviceProvider = serviceProvider;
    }

    public async Task EnsureIntegrityAsync()
    {
        _context.ChangeTracker.Clear();
        await EnsureAdminUserAsync();

        var adminUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "admin");

        if (adminUser == null)
        {
            var errorMessage = "🔥 FALLO CRÍTICO: Usuario 'admin' existe pero no se pudo cargar. Estado inconsistente detectado.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
        if (adminUser.CompanyId == Guid.Empty || adminUser.CompanyId == default(Guid))
        {
            var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' no tiene CompanyId vinculado (CompanyId: {adminUser.CompanyId}). El sistema sería inaccesible. Revise la vinculación en demo-data.json.";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
        string? companyName = null;

        // Empresa: verificar vía Admin API (Product no conoce BD de empresa)
        var adminClient = _serviceProvider.GetService<IAdminApiClient>();

        if (adminClient != null)
        {
            var company = await adminClient.GetCompanyAsync(adminUser.CompanyId);
            if (company == null)
            {
                var errorMessage = $"🔥 FALLO CRÍTICO DE INTEGRIDAD REFERENCIAL: El usuario 'admin' tiene CompanyId ({adminUser.CompanyId}) pero la empresa no existe en Admin API. Revise la vinculación en demo-data.json.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
            companyName = company.Name;
        }

        const string EXPECTED_ADMIN_COMPANY_NAME = "Empresa Admin";
        const string EXPECTED_ADMIN_COMPANY_ID = "550e8400-e29b-41d4-a716-446655440000";
        if (companyName != null && companyName != EXPECTED_ADMIN_COMPANY_NAME)
        {
            _logger.LogWarning("⚠️ ADVERTENCIA: El usuario 'admin' está vinculado a '{Name}' en lugar de '{Expected}'.", companyName, EXPECTED_ADMIN_COMPANY_NAME);
        }
        if (adminUser.CompanyId.ToString() != EXPECTED_ADMIN_COMPANY_ID)
        {
            _logger.LogWarning("⚠️ ADVERTENCIA: El usuario 'admin' tiene CompanyId '{Id}' en lugar del esperado '{Expected}'.", adminUser.CompanyId, EXPECTED_ADMIN_COMPANY_ID);
        }
        var companyInfo = $" (Empresa: {companyName ?? adminUser.CompanyId.ToString()}, CompanyId: {adminUser.CompanyId})";
        _logger.LogInformation("✅ Smoke Test Superado: Usuario 'admin' verificado correctamente{CompanyInfo}", companyInfo);
    }

    private async Task EnsureAdminUserAsync()
    {
        var isTesting = _environment.EnvironmentName == "Testing";
        const string TestAdminPassword = "admin123";

        const string AdminUsername = "admin";

        // ID de empresa demo por si el admin viene de seeds con CompanyId vacío (solo reparación)
        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");

        async Task EnsureCoreAsync()
        {
            var localAdmin = _context.Users.Local.FirstOrDefault(u => u.Username == AdminUsername);
            if (localAdmin != null)
            {
                if (localAdmin.DeletedAt != null)
                {
                    localAdmin.DeletedAt = null;
                    localAdmin.IsActive = true;
                }
                if (string.IsNullOrWhiteSpace(localAdmin.PasswordHash))
                {
                    var newPwd = isTesting ? TestAdminPassword : _sanitizer.GenerateRandomPassword();
                    localAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    _logger.LogWarning("[ENSURE ADMIN] 🔐 Set password for existing local admin: {Kind}", isTesting ? "admin123 (Testing)" : "random");
                }
                if (localAdmin.CompanyId == Guid.Empty || localAdmin.CompanyId == default(Guid))
                {
                    localAdmin.CompanyId = defaultCompanyId;
                }
                await _context.SaveChangesAsync();
                return;
            }

            var admin = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == AdminUsername);

            if (admin != null)
            {
                if (admin.DeletedAt != null)
                {
                    admin.DeletedAt = null;
                    admin.IsActive = true;
                }
                if (string.IsNullOrWhiteSpace(admin.PasswordHash))
                {
                    var newPwd = isTesting ? TestAdminPassword : _sanitizer.GenerateRandomPassword();
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPwd);
                    _logger.LogWarning("[ENSURE ADMIN] 🔐 Set password for existing admin (was empty): {Kind}", isTesting ? "admin123 (Testing)" : "random");
                }
                if (admin.CompanyId == Guid.Empty || admin.CompanyId == default(Guid))
                {
                    admin.CompanyId = defaultCompanyId;
                }

                // Empresa: validar vía Admin API
                var adminClient = _serviceProvider.GetService<IAdminApiClient>();
                if (adminClient != null)
                {
                    var company = await adminClient.GetCompanyAsync(admin.CompanyId);
                    if (company == null)
                    {
                        _logger.LogError("El usuario 'admin' tiene CompanyId {CompanyId} pero la empresa no existe en Admin API.", admin.CompanyId);
                        throw new InvalidOperationException("El usuario 'admin' está referenciando una empresa inexistente. Ejecute seeds de Admin.");
                    }
                }

                await _context.SaveChangesAsync();
                return;
            }

            // Admin no existe
            _logger.LogError("El usuario 'admin' no existe en la BD. Debe estar definido en los seeds (demo-data.json o test-data.json).");
            throw new InvalidOperationException(
                "El usuario 'admin' debe estar definido en los seeds (demo-data.json o test-data.json). Toda la carga masiva de empresas y usuarios se realiza mediante seeds.");
        }

        if (_context.Database.IsRelational())
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                await EnsureCoreAsync();
                await tx.CommitAsync();
            });
            return;
        }

        await EnsureCoreAsync();
    }
}
