using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa una provincia/estado
/// </summary>
public class State : BaseEntity
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; } // Código de la provincia/estado (opcional)

    // Navegación
    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = new List<City>();
}

