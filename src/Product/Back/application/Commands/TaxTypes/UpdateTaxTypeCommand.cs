using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Commands.TaxTypes;

// Fix: Update returns Result (no value), so implement non-generic ICommand
public record UpdateTaxTypeCommand(Guid Id, UpdateTaxTypeDto TaxType) : ICommand;
