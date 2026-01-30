using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.PostalCode;

public record DeletePostalCodeCommand(Guid Id) : ICommand;

