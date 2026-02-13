using GesFer.Product.Application.Commands.TaxTypes;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.Application.Queries.TaxTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesFer.Product.Back.Api.Controllers;

[Route("api/tax-types")]
[ApiController]
[Authorize]
public class TaxTypesController : ControllerBase
{
    private readonly ISender _sender;

    public TaxTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaxTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaxTypes(CancellationToken cancellationToken)
    {
        var query = new GetTaxTypesQuery();
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaxTypeById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaxTypeByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTaxType([FromBody] CreateTaxTypeDto request, CancellationToken cancellationToken)
    {
        var command = new CreateTaxTypeCommand(request);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetTaxTypeById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTaxType(Guid id, [FromBody] UpdateTaxTypeDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        var command = new UpdateTaxTypeCommand(id, request);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code == "TaxType.NotFound"
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaxType(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteTaxTypeCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return NoContent();
    }
}
