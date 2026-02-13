using System.Security.Claims;
using GesFer.Application.Commands.TaxTypes;
using GesFer.Application.Common.Interfaces;
using GesFer.Product.Application.DTOs.TaxTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Api.Controllers;

[Route("api/tax-types")]
[ApiController]
[Authorize]
public class TaxTypesController : ControllerBase
{
    private readonly ICommandHandler<GetAllTaxTypesCommand, List<TaxTypeDto>> _getAllHandler;
    private readonly ICommandHandler<GetTaxTypeByIdCommand, TaxTypeDto?> _getByIdHandler;
    private readonly ICommandHandler<CreateTaxTypeCommand, Guid> _createHandler;
    private readonly ICommandHandler<UpdateTaxTypeCommand> _updateHandler;
    private readonly ICommandHandler<DeleteTaxTypeCommand> _deleteHandler;

    public TaxTypesController(
        ICommandHandler<GetAllTaxTypesCommand, List<TaxTypeDto>> getAllHandler,
        ICommandHandler<GetTaxTypeByIdCommand, TaxTypeDto?> getByIdHandler,
        ICommandHandler<CreateTaxTypeCommand, Guid> createHandler,
        ICommandHandler<UpdateTaxTypeCommand> updateHandler,
        ICommandHandler<DeleteTaxTypeCommand> deleteHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("company_id")?.Value ?? User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            throw new UnauthorizedAccessException("No se encontró el ID de empresa en el token del usuario.");
        return companyId;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TaxTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxTypes(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var command = new GetAllTaxTypesCommand(companyId);
            var result = await _getAllHandler.HandleAsync(command, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxTypeById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var command = new GetTaxTypeByIdCommand(id, companyId);
            var result = await _getByIdHandler.HandleAsync(command, cancellationToken);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTaxType([FromBody] CreateTaxTypeDto request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var command = new CreateTaxTypeCommand(request, companyId);
            var id = await _createHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetTaxTypeById), new { id }, id);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTaxType(Guid id, [FromBody] UpdateTaxTypeDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("ID mismatch");

        try
        {
            var companyId = GetCompanyId();
            var command = new UpdateTaxTypeCommand(request, companyId);
            await _updateHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { message = ex.Message })
                : BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaxType(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var command = new DeleteTaxTypeCommand(id, companyId);
            await _deleteHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
