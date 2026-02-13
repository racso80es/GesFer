using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Queries.TaxTypes;

// Fix: TResponse is IReadOnlyList<TaxTypeDto>, wrapper Result<> is added by IQuery
public record GetTaxTypesQuery() : IQuery<IReadOnlyList<TaxTypeDto>>;

// Fix: TResponse is TaxTypeDto
public record GetTaxTypeByIdQuery(Guid Id) : IQuery<TaxTypeDto>;
