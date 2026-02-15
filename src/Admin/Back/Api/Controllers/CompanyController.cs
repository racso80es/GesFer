using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.DTOs.Company;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Admin.Api.Controllers;

/// <summary>
/// Controlador para gestión de empresas en Admin
/// </summary>
using GesFer.Admin.Api.Attributes;

[ApiController]
[Route("api/[controller]")]
[AuthorizeSystemOrAdmin] // Protegido por Shared Secret O rol Admin
public class CompanyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(IMediator mediator, ILogger<CompanyController> logger)
    {
        _mediator = mediator;
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
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una empresa por nombre (para Product: login y contexto de una sola empresa).
    /// </summary>
    [HttpGet("by-name")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "El parámetro name es obligatorio" });
        try
        {
            var command = new GetCompanyByNameCommand(name);
            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound(new { message = $"No se encontró la empresa con nombre '{name}'" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa por nombre {Name}", name);
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
            var result = await _mediator.Send(command);

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
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            await _mediator.Send(command);
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
