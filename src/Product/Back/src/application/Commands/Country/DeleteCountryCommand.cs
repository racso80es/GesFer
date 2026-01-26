using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.Country;

public record DeleteCountryCommand(Guid Id) : ICommand;

