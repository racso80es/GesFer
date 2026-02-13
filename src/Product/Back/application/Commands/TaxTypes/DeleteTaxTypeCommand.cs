using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Commands.TaxTypes;

// Fix: TResponse should be void/Unit implies generic ICommand returning Result, but Delete returns Result (no value)
// So it should implement non-generic ICommand
public record DeleteTaxTypeCommand(Guid Id) : ICommand;
