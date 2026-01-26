using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;

namespace GesFer.Application.Commands.User;

public record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : ICommand<UserDto>;

