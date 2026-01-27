using GesFer.Application.Common.Interfaces;
using GesFer.Application.DTOs.Group;

namespace GesFer.Application.Commands.Group;

public record GetAllGroupsCommand() : ICommand<List<GroupDto>>;

