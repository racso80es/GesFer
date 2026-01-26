using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Tabla de relación Many-to-Many entre Group y Permission
/// </summary>
public class GroupPermission : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid PermissionId { get; set; }

    // Navegación
    public Group Group { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

