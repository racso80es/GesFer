using GesFer.Application.Commands.City;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.City;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de ciudades
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CityController : ControllerBase
{
    private readonly ICommandHandler<CreateCityCommand, CityDto> _createHandler;
    private readonly ICommandHandler<UpdateCityCommand, CityDto> _updateHandler;
    private readonly ICommandHandler<DeleteCityCommand> _deleteHandler;
    private readonly ICommandHandler<GetCityByIdCommand, CityDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllCitiesCommand, List<CityDto>> _getAllHandler;
    private readonly ILogger<CityController> _logger;

    public CityController(
        ICommandHandler<CreateCityCommand, CityDto> createHandler,
        ICommandHandler<UpdateCityCommand, CityDto> updateHandler,
        ICommandHandler<DeleteCityCommand> deleteHandler,
        ICommandHandler<GetCityByIdCommand, CityDto?> getByIdHandler,
        ICommandHandler<GetAllCitiesCommand, List<CityDto>> getAllHandler,
        ILogger<CityController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las ciudades, opcionalmente filtradas por provincia o país
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? stateId = null, [FromQuery] Guid? countryId = null)
    {
        try
        {
            var command = new GetAllCitiesCommand(stateId, countryId);
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ciudades");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una ciudad por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetCityByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró la ciudad con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ciudad {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva ciudad
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
    {
        try
        {
            var command = new CreateCityCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ciudad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una ciudad existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCityDto dto)
    {
        try
        {
            var command = new UpdateCityCommand(id, dto);
            var result = await _updateHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("No se encontró"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar ciudad {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina una ciudad (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCityCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ciudad {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

