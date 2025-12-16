using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Tabla de relación Many-to-Many entre User y Permission (permisos directos)
/// </summary>
public class UserPermission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

