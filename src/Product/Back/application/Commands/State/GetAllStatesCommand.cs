using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;

namespace GesFer.Application.Commands.State;

public record GetAllStatesCommand(Guid? CountryId = null) : ICommand<List<StateDto>>;

