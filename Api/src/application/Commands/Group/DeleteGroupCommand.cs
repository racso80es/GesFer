using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.Group;

public record DeleteGroupCommand(Guid Id) : ICommand;

