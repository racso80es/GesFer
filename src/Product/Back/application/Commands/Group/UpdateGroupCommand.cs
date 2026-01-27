using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;

namespace GesFer.Application.Commands.Group;

public record UpdateGroupCommand(Guid Id, UpdateGroupDto Dto) : ICommand<GroupDto>;

