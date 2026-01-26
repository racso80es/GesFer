using GesFer.Domain.Common;

namespace GesFer.Domain.Entities;

/// <summary>
/// Entidad que representa un permiso del sistema
/// </summary>
public class Permission : BaseEntity
{
    public string Key { get; set; } = string.Empty; // Clave única del permiso
    public string Description { get; set; } = string.Empty;

    // Navegación
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
}

