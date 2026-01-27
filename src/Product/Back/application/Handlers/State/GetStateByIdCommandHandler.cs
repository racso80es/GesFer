using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.State;

public class GetStateByIdCommandHandler : ICommandHandler<GetStateByIdCommand, StateDto?>
{
    private readonly ApplicationDbContext _context;

    public GetStateByIdCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StateDto?> HandleAsync(GetStateByIdCommand command, CancellationToken cancellationToken = default)
    {
        var state = await _context.States
            .Include(s => s.Country)
            .Where(s => s.Id == command.Id && s.DeletedAt == null)
            .Select(s => new StateDto
            {
                Id = s.Id,
                CountryId = s.CountryId,
                CountryName = s.Country.Name,
                CountryCode = s.Country.Code,
                Name = s.Name,
                Code = s.Code,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return state;
    }
}

