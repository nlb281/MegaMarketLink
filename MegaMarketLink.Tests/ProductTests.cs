using MegaMarketLink.Models;

namespace MegaMarketLink.Tests;

public class ProductTests
{
    [Theory]
    [InlineData(true, "Снять с публикации")]
    [InlineData(false, "Опубликовать")]
    public void PublishButtonText_DependsOnPublicationStatus(bool isActive, string expected)
    {
        // Arrange
        var product = new Product { IsActive = isActive };

        // Act
        var buttonText = product.PublishButtonText;

        // Assert
        Assert.Equal(expected, buttonText);
    }
}
