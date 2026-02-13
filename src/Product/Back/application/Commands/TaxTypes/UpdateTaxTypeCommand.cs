using GesFer.Application.Common.Interfaces;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Application.Commands.TaxTypes;

public record UpdateTaxTypeCommand(UpdateTaxTypeDto TaxType, Guid? CompanyId = null) : ICommand;
