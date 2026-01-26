using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Tabla de relación Many-to-Many entre User y Group
/// </summary>
public class UserGroup : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public Group Group { get; set; } = null!;
}

