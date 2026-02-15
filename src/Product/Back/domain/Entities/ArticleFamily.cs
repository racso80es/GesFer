using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad que representa una familia de artículos (maestro).
/// Sustituye a la antigua Family; cada familia se asocia a un TaxType para el tratamiento fiscal.
/// </summary>
public class ArticleFamily : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TaxTypeId { get; set; }

    // CompanyId FK a Admin
    public TaxType TaxType { get; set; } = null!;
}
