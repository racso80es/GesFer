using GesFer.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para inicialización y configuración del entorno
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly ISetupService _setupService;
    private readonly ILogger<SetupController> _logger;

    public SetupController(
        ISetupService setupService,
        ILogger<SetupController> logger)
    {
        _setupService = setupService;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa todo el entorno: elimina contenedores Docker, los recrea, crea la base de datos e inserta datos iniciales
    /// </summary>
    /// <returns>Resultado de la inicialización con los pasos ejecutados</returns>
    /// <remarks>
    /// Este endpoint realiza las siguientes acciones:
    /// 1. Detiene y elimina los contenedores Docker del proyecto
    /// 2. Limpia los volúmenes Docker
    /// 3. Recrea los contenedores con docker-compose
    /// 4. Espera a que MySQL esté listo
    /// 5. Crea la base de datos y todas las tablas
    /// 6. Inserta los datos iniciales desde seed-data.sql
    /// 
    /// ⚠️ ADVERTENCIA: Este endpoint elimina todos los datos existentes en la base de datos.
    /// </remarks>
    [HttpPost("initialize")]
    [ProducesResponseType(typeof(SetupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SetupResult), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Initialize()
    {
        try
        {
            _logger.LogWarning("Iniciando inicialización completa del entorno...");
            
            var result = await _setupService.InitializeEnvironmentAsync();

            if (result.Success)
            {
                _logger.LogInformation("Inicialización completada exitosamente");
                return Ok(result);
            }
            else
            {
                _logger.LogError("Error durante la inicialización: {Errors}", string.Join(", ", result.Errors));
                return StatusCode(500, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante la inicialización");
            return StatusCode(500, new SetupResult
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Obtiene el estado actual del entorno
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            message = "Endpoint de inicialización disponible",
            endpoint = "/api/setup/initialize",
            warning = "Este endpoint elimina todos los datos existentes"
        });
    }
}

