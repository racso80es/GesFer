using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;

namespace GesFer.Application.Commands.Group;

public record GetGroupByIdCommand(Guid Id) : ICommand<GroupDto?>;

