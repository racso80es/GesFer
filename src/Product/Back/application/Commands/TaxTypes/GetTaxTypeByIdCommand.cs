using GesFer.Application.Common.Interfaces;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Application.Commands.TaxTypes;

public record GetTaxTypeByIdCommand(Guid Id, Guid? CompanyId = null) : ICommand<TaxTypeDto?>;
