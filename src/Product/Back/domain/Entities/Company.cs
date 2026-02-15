using GesFer.Shared.Back.Domain.Entities;
using System.Collections.Generic;

namespace GesFer.Product.Back.Domain.Entities;

public class Company : GesFer.Shared.Back.Domain.Entities.Company
{
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
