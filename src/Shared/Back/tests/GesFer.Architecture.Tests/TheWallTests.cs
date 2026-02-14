using NetArchTest.Rules;
using Xunit;
using GesFer.Api.Controllers;
using GesFer.Admin.Api.Controllers;
using System.Reflection;

namespace GesFer.Architecture.Tests;

public class TheWallTests
{
    [Fact]
    public void Product_Api_Should_Not_Depend_On_Admin()
    {
        var result = Types.InAssembly(typeof(AuthController).Assembly)
            .ShouldNot()
            .HaveDependencyOn("GesFer.Admin")
            .GetResult();

        Assert.True(result.IsSuccessful, "GesFer.Api (Product) should not depend on GesFer.Admin namespaces.");
    }

    [Fact]
    public void Product_Domain_Should_Not_Depend_On_Infrastructure()
    {
        var domainAssembly = typeof(GesFer.Product.Back.Domain.Entities.Article).Assembly;
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("GesFer.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, "GesFer.Domain (Product) should not depend on GesFer.Infrastructure.");
    }

    [Fact]
    public void Admin_Domain_Should_Not_Depend_On_Infrastructure()
    {
        var domainAssembly = typeof(GesFer.Admin.Back.Domain.Entities.AdminUser).Assembly;
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("GesFer.Admin.Infra")
            .GetResult();

        Assert.True(result.IsSuccessful, "GesFer.Admin.Domain should not depend on GesFer.Admin.Infra.");
    }

    // NOTE: Admin depends on Product for Dashboard (ReadOnly), so we only enforce Product -> Admin isolation.
    // [Fact]
    // public void Admin_Api_Should_Not_Depend_On_Product()
    // {
    //    ...
    // }
}
