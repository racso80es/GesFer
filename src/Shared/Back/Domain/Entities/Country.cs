using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Shared.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un país
/// </summary>
public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Código ISO (ej: ES, US, MX)
    public Guid LanguageId { get; set; }
    public Language? Language { get; set; }

    // Navegación
    public ICollection<State> States { get; set; } = new List<State>();
}
