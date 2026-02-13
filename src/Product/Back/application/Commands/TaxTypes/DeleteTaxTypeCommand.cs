using GesFer.Shared.Back.Application.Abstractions.Messaging;

namespace GesFer.Product.Application.Commands.TaxTypes;

public record DeleteTaxTypeCommand(Guid Id) : ICommand<Result>;
