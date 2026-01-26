using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.State;

public record DeleteStateCommand(Guid Id) : ICommand;

