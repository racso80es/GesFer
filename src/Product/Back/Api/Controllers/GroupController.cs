using GesFer.Application.Commands.Group;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de grupos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly ICommandHandler<CreateGroupCommand, GroupDto> _createHandler;
    private readonly ICommandHandler<UpdateGroupCommand, GroupDto> _updateHandler;
    private readonly ICommandHandler<DeleteGroupCommand> _deleteHandler;
    private readonly ICommandHandler<GetGroupByIdCommand, GroupDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllGroupsCommand, List<GroupDto>> _getAllHandler;
    private readonly ILogger<GroupController> _logger;

    public GroupController(
        ICommandHandler<CreateGroupCommand, GroupDto> createHandler,
        ICommandHandler<UpdateGroupCommand, GroupDto> updateHandler,
        ICommandHandler<DeleteGroupCommand> deleteHandler,
        ICommandHandler<GetGroupByIdCommand, GroupDto?> getByIdHandler,
        ICommandHandler<GetAllGroupsCommand, List<GroupDto>> getAllHandler,
        ILogger<GroupController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los grupos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var command = new GetAllGroupsCommand();
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener grupos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un grupo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetGroupByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró el grupo con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener grupo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo grupo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
    {
        try
        {
            var command = new CreateGroupCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear grupo");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un grupo existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupDto dto)
    {
        try
        {
            var command = new UpdateGroupCommand(id, dto);
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
            _logger.LogError(ex, "Error al actualizar grupo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un grupo (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteGroupCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar grupo {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

