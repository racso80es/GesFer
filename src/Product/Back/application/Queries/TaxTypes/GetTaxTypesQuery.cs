using GesFer.Shared.Back.Application.Abstractions.Messaging;
using GesFer.Product.Application.DTOs.TaxTypes;

namespace GesFer.Product.Application.Queries.TaxTypes;

public record GetTaxTypesQuery() : IQuery<IReadOnlyList<TaxTypeDto>>;
public record GetTaxTypeByIdQuery(Guid Id) : IQuery<TaxTypeDto>;
