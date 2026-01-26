using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Shared.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un código postal
/// </summary>
public class PostalCode : BaseEntity
{
    public Guid CityId { get; set; }
    public string Code { get; set; } = string.Empty; // Código postal (ej: 28001, 08001)

    // Navegación
    public City City { get; set; } = null!;
}
