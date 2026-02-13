using GesFer.Application.Common.Interfaces;

namespace GesFer.Application.Commands.TaxTypes;

public record DeleteTaxTypeCommand(Guid Id, Guid? CompanyId = null) : ICommand;
