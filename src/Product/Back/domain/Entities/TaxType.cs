using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un tipo de tasa impositiva (ej. IVA 21%, IVA 10%).
/// </summary>
public class TaxType : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty; // Código único por empresa (ej. "IVA21")
    public string Name { get; set; } = string.Empty; // Nombre único por empresa (ej. "IVA General 21%")
    public string? Description { get; set; }
    public decimal Value { get; set; } // Valor porcentual (ej. 21.00 para 21%)

    // CompanyId FK a Admin; sin navegación
}
