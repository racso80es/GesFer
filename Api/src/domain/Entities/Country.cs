using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un país
/// </summary>
public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Código ISO (ej: ES, US, MX)

    // Navegación
    public ICollection<State> States { get; set; } = new List<State>();
}

