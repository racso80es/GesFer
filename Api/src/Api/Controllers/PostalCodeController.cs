using GesFer.Application.Commands.PostalCode;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.PostalCode;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de códigos postales
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostalCodeController : ControllerBase
{
    private readonly ICommandHandler<CreatePostalCodeCommand, PostalCodeDto> _createHandler;
    private readonly ICommandHandler<UpdatePostalCodeCommand, PostalCodeDto> _updateHandler;
    private readonly ICommandHandler<DeletePostalCodeCommand> _deleteHandler;
    private readonly ICommandHandler<GetPostalCodeByIdCommand, PostalCodeDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllPostalCodesCommand, List<PostalCodeDto>> _getAllHandler;
    private readonly ILogger<PostalCodeController> _logger;

    public PostalCodeController(
        ICommandHandler<CreatePostalCodeCommand, PostalCodeDto> createHandler,
        ICommandHandler<UpdatePostalCodeCommand, PostalCodeDto> updateHandler,
        ICommandHandler<DeletePostalCodeCommand> deleteHandler,
        ICommandHandler<GetPostalCodeByIdCommand, PostalCodeDto?> getByIdHandler,
        ICommandHandler<GetAllPostalCodesCommand, List<PostalCodeDto>> getAllHandler,
        ILogger<PostalCodeController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los códigos postales, opcionalmente filtrados por ciudad, provincia o país
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PostalCodeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? cityId = null, [FromQuery] Guid? stateId = null, [FromQuery] Guid? countryId = null)
    {
        try
        {
            var command = new GetAllPostalCodesCommand(cityId, stateId, countryId);
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener códigos postales");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un código postal por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostalCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetPostalCodeByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró el código postal con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener código postal {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo código postal
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PostalCodeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePostalCodeDto dto)
    {
        try
        {
            var command = new CreatePostalCodeCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear código postal");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un código postal existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PostalCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostalCodeDto dto)
    {
        try
        {
            var command = new UpdatePostalCodeCommand(id, dto);
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
            _logger.LogError(ex, "Error al actualizar código postal {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un código postal (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeletePostalCodeCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar código postal {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

