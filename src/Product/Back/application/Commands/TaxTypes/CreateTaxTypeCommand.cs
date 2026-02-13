using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Commands.TaxTypes;

// Fix: TResponse should be Guid, not Result<Guid> because ICommand<T> wraps it in Result<T>
public record CreateTaxTypeCommand(CreateTaxTypeDto TaxType) : ICommand<Guid>;
