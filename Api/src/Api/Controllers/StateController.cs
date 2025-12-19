using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de provincias/estados
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StateController : ControllerBase
{
    private readonly ICommandHandler<CreateStateCommand, StateDto> _createHandler;
    private readonly ICommandHandler<UpdateStateCommand, StateDto> _updateHandler;
    private readonly ICommandHandler<DeleteStateCommand> _deleteHandler;
    private readonly ICommandHandler<GetStateByIdCommand, StateDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllStatesCommand, List<StateDto>> _getAllHandler;
    private readonly ILogger<StateController> _logger;

    public StateController(
        ICommandHandler<CreateStateCommand, StateDto> createHandler,
        ICommandHandler<UpdateStateCommand, StateDto> updateHandler,
        ICommandHandler<DeleteStateCommand> deleteHandler,
        ICommandHandler<GetStateByIdCommand, StateDto?> getByIdHandler,
        ICommandHandler<GetAllStatesCommand, List<StateDto>> getAllHandler,
        ILogger<StateController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las provincias/estados, opcionalmente filtradas por país
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<StateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? countryId = null)
    {
        try
        {
            var command = new GetAllStatesCommand(countryId);
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener provincias/estados");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una provincia/estado por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetStateByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró la provincia/estado con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener provincia/estado {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva provincia/estado
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStateDto dto)
    {
        try
        {
            var command = new CreateStateCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear provincia/estado");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una provincia/estado existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStateDto dto)
    {
        try
        {
            var command = new UpdateStateCommand(id, dto);
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
            _logger.LogError(ex, "Error al actualizar provincia/estado {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina una provincia/estado (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteStateCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("No se encontró"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar provincia/estado {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

