using System.Collections.Generic;
using System.Linq;
using MockQueryable.Moq;
using Moq;

namespace GesFer.Product.UnitTests.Infrastructure;

public static class MockDbSetExtensions
{
    public static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> BuildMockDbSet<T>(this IEnumerable<T> source) where T : class
    {
        return source.AsQueryable().BuildMockDbSet();
    }
}
