using FluentAssertions;
using GesFer.Product.Back.Domain.Entities;
using Xunit;

namespace GesFer.Product.UnitTests.Tariffs;

public class TariffItemTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tariffId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var price = 10.5m;

        // Act
        var item = new TariffItem
        {
            Id = id,
            TariffId = tariffId,
            ArticleId = articleId,
            Price = price
        };

        // Assert
        item.Id.Should().Be(id);
        item.TariffId.Should().Be(tariffId);
        item.ArticleId.Should().Be(articleId);
        item.Price.Should().Be(price);
    }
}
