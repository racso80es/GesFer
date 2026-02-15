using GesFer.Shared.Back.Domain.Common;
using GesFer.Shared.Back.Domain.ValueObjects;
using GesFer.Shared.Back.Domain.Entities;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad que representa un usuario del sistema
/// </summary>
public class User : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Email? Email { get; set; }
    public string? Phone { get; set; }

    // Campos de dirección (opcionales)
    public string? Address { get; set; }
    public Guid? PostalCodeId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? LanguageId { get; set; }

    // Navegación (CompanyId FK a Companies en Admin; datos de empresa vía Admin API)
    public GesFer.Shared.Back.Domain.Entities.PostalCode? PostalCode { get; set; }
    public GesFer.Shared.Back.Domain.Entities.City? City { get; set; }
    public GesFer.Shared.Back.Domain.Entities.State? State { get; set; }
    public GesFer.Shared.Back.Domain.Entities.Country? Country { get; set; }
    public GesFer.Shared.Back.Domain.Entities.Language? Language { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

