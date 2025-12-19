using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.User;

namespace GesFer.Application.Commands.User;

public record CreateUserCommand(CreateUserDto Dto) : ICommand<UserDto>;

