using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una familia de artículos
/// </summary>
public class Family : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal IvaPercentage { get; set; } // Porcentaje de IVA (obligatorio)

    // Navegación
    public Company Company { get; set; } = null!;
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

