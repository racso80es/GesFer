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

    // NOTE: Admin depends on Product for Dashboard (ReadOnly), so we only enforce Product -> Admin isolation.
    // [Fact]
    // public void Admin_Api_Should_Not_Depend_On_Product()
    // {
    //    ...
    // }
}
