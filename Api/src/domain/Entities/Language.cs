using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Idioma maestro del sistema.
/// </summary>
public class Language : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // abreviatura ISO (es, en, ca)
    public string? Description { get; set; }

    // Navegación inversa opcional
    public ICollection<Country> Countries { get; set; } = new List<Country>();
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ICollection<User> Users { get; set; } = new List<User>();
}


