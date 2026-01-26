using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un precio específico de un artículo en una tarifa
/// </summary>
public class TariffItem : BaseEntity
{
    public Guid TariffId { get; set; }
    public Guid ArticleId { get; set; }
    public decimal Price { get; set; } // Precio específico para este artículo en esta tarifa

    // Navegación
    public Tariff Tariff { get; set; } = null!;
    public Article Article { get; set; } = null!;
}

