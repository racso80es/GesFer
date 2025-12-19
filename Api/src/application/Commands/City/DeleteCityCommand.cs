using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.City;

public record DeleteCityCommand(Guid Id) : ICommand;

