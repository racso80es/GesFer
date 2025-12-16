namespace GesFer.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Implementa Soft Delete y auditoría básica.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indica si la entidad está eliminada (soft delete)
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;
}

