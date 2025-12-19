using GesFer.Application.Commands.User;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de usuarios
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ICommandHandler<CreateUserCommand, UserDto> _createHandler;
    private readonly ICommandHandler<UpdateUserCommand, UserDto> _updateHandler;
    private readonly ICommandHandler<DeleteUserCommand> _deleteHandler;
    private readonly ICommandHandler<GetUserByIdCommand, UserDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllUsersCommand, List<UserDto>> _getAllHandler;
    private readonly ILogger<UserController> _logger;

    public UserController(
        ICommandHandler<CreateUserCommand, UserDto> createHandler,
        ICommandHandler<UpdateUserCommand, UserDto> updateHandler,
        ICommandHandler<DeleteUserCommand> deleteHandler,
        ICommandHandler<GetUserByIdCommand, UserDto?> getByIdHandler,
        ICommandHandler<GetAllUsersCommand, List<UserDto>> getAllHandler,
        ILogger<UserController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios, opcionalmente filtrados por empresa
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId = null)
    {
        try
        {
            var command = new GetAllUsersCommand(companyId);
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetUserByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró el usuario con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var command = new CreateUserCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var command = new UpdateUserCommand(id, dto);
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
            _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un usuario (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteUserCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

