using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.Company;

public record DeleteCompanyCommand(Guid Id) : ICommand;

