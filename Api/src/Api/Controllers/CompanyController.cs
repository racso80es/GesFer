using GesFer.Application.Commands.Company;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Company;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

/// <summary>
/// Controlador para gestión de empresas
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICommandHandler<CreateCompanyCommand, CompanyDto> _createHandler;
    private readonly ICommandHandler<UpdateCompanyCommand, CompanyDto> _updateHandler;
    private readonly ICommandHandler<DeleteCompanyCommand> _deleteHandler;
    private readonly ICommandHandler<GetCompanyByIdCommand, CompanyDto?> _getByIdHandler;
    private readonly ICommandHandler<GetAllCompaniesCommand, List<CompanyDto>> _getAllHandler;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(
        ICommandHandler<CreateCompanyCommand, CompanyDto> createHandler,
        ICommandHandler<UpdateCompanyCommand, CompanyDto> updateHandler,
        ICommandHandler<DeleteCompanyCommand> deleteHandler,
        ICommandHandler<GetCompanyByIdCommand, CompanyDto?> getByIdHandler,
        ICommandHandler<GetAllCompaniesCommand, List<CompanyDto>> getAllHandler,
        ILogger<CompanyController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las empresas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var command = new GetAllCompaniesCommand();
            var result = await _getAllHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una empresa por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var command = new GetCompanyByIdCommand(id);
            var result = await _getByIdHandler.HandleAsync(command);

            if (result == null)
                return NotFound(new { message = $"No se encontró la empresa con ID {id}" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva empresa
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
    {
        try
        {
            var command = new CreateCompanyCommand(dto);
            var result = await _createHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una empresa existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        try
        {
            var command = new UpdateCompanyCommand(id, dto);
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
            _logger.LogError(ex, "Error al actualizar empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina una empresa (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCompanyCommand(id);
            await _deleteHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

