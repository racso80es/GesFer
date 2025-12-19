using GesFer.Application.Commands.State;
using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Application.Handlers.State;

public class GetAllStatesCommandHandler : ICommandHandler<GetAllStatesCommand, List<StateDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllStatesCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StateDto>> HandleAsync(GetAllStatesCommand command, CancellationToken cancellationToken = default)
    {
        var query = _context.States
            .Include(s => s.Country)
            .Where(s => s.DeletedAt == null);

        // Filtrar por CountryId si se proporciona
        if (command.CountryId.HasValue)
        {
            query = query.Where(s => s.CountryId == command.CountryId.Value);
        }

        var states = await query
            .OrderBy(s => s.Country.Name)
            .ThenBy(s => s.Name)
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
            .ToListAsync(cancellationToken);

        return states;
    }
}

