using GesFer.Shared.Back.Domain.Entities;

namespace GesFer.Product.Back.Domain.Entities;

/// <summary>
/// Entidad extendida de Company para el dominio de Product.
/// Agrega colecciones de negocio específicas.
/// </summary>
public class Company : GesFer.Shared.Back.Domain.Entities.Company
{
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<ArticleFamily> ArticleFamilies { get; set; } = new List<ArticleFamily>();
    public ICollection<TaxType> TaxTypes { get; set; } = new List<TaxType>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
