namespace GesFer.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Implementa Soft Delete y auditoría básica.
/// 
/// Nota: El Id se genera automáticamente mediante SequentialGuidValueGenerator
/// configurado en ApplicationDbContext para mejorar el rendimiento de índices agrupados.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad.
    /// Se genera automáticamente como GUID secuencial (COMB GUID) por EF Core
    /// mediante SequentialGuidValueGenerator para optimizar el rendimiento de índices.
    /// </summary>
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indica si la entidad está eliminada (soft delete)
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;
}

