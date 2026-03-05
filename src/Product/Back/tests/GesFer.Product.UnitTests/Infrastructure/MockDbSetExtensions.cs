using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;

namespace GesFer.Product.UnitTests.Infrastructure
{
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IEnumerable<T> sourceList) where T : class
        {
            return sourceList.AsQueryable().BuildMockDbSet();
        }
    }
}
