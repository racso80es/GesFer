namespace GesFer.Api.Services;

/// <summary>
/// Servicio para inicializar el entorno completo (Docker, base de datos, datos iniciales)
/// </summary>
public interface ISetupService
{
    /// <summary>
    /// Inicializa todo el entorno: elimina contenedores, recrea Docker, crea BD y datos iniciales
    /// </summary>
    Task<SetupResult> InitializeEnvironmentAsync();
}

/// <summary>
/// Resultado de la inicialización
/// </summary>
public class SetupResult
{
    public bool Success { get; set; }
    public List<string> Steps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
}

