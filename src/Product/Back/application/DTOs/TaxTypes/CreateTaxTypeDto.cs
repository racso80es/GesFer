namespace GesFer.Product.Application.DTOs.TaxTypes;

public record CreateTaxTypeDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Value { get; init; }
}
