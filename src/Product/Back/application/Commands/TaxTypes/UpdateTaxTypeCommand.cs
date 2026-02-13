using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Commands.TaxTypes;

public record UpdateTaxTypeCommand(UpdateTaxTypeDto TaxType) : ICommand<Result>;
