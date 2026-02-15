using Microsoft.Extensions.Configuration;

namespace GesFer.Infrastructure.Configuration;

/// <summary>
/// Configuración estática de seeds. CompanyId viene de config (Product no consulta Admin para seeds).
/// Agregar en appsettings.json: "Seed": { "CompanyId": "11111111-1111-1111-1111-111111111115" }
/// </summary>
public static class SeedConfig
{
    private const string KeyCompanyId = "Seed:CompanyId";

    /// <summary>
    /// Obtiene el CompanyId válido para seeds desde configuración.
    /// Clave: Seed:CompanyId (Guid). Si no está definido o es inválido, devuelve Guid.Empty.
    /// </summary>
    public static Guid GetCompanyId(IConfiguration config)
    {
        var s = config[KeyCompanyId];
        return Guid.TryParse(s, out var g) ? g : Guid.Empty;
    }

    /// <summary>
    /// Devuelve un HashSet con el CompanyId de config (o vacío si no está definido).
    /// Para uso en JsonDataSeeder como validCompanyIds.
    /// </summary>
    public static HashSet<Guid> GetValidCompanyIds(IConfiguration config)
    {
        var id = GetCompanyId(config);
        return id == Guid.Empty ? new HashSet<Guid>() : new HashSet<Guid> { id };
    }
}
