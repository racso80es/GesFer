using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.User;

public record DeleteUserCommand(Guid Id) : ICommand;

