using GesFer.Shared.Back.Domain.Common;

namespace GesFer.Shared.Back.Domain.Entities;

/// <summary>
/// Idioma maestro del sistema.
/// </summary>
public class Language : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // abreviatura ISO (es, en, ca)
    public string? Description { get; set; }

    // Navegación inversa opcional
    // Nota: Las referencias a Company y User se manejan en Product mediante navegación inversa
    public ICollection<Country> Countries { get; set; } = new List<Country>();
}
