using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.State;

namespace GesFer.Application.Commands.State;

public record UpdateStateCommand(Guid Id, UpdateStateDto Dto) : ICommand<StateDto>;

