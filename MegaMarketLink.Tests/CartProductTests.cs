using System.ComponentModel;
using MegaMarketLink.Models;

namespace MegaMarketLink.Tests;

public class CartProductTests
{
    [Fact]
    public void TotalPrice_WithPriceAndQuantity_ReturnsProductOfValues()
    {
        // Arrange
        var cartProduct = new CartProduct { Price = 199.50m, Quantity = 3 };

        // Act
        var totalPrice = cartProduct.TotalPrice;

        // Assert
        Assert.Equal(598.50m, totalPrice);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void CanDecrease_DependsOnQuantity(int quantity, bool expected)
    {
        // Arrange
        var cartProduct = new CartProduct { Quantity = quantity };

        // Act
        var canDecrease = cartProduct.CanDecrease;

        // Assert
        Assert.Equal(expected, canDecrease);
    }

    [Fact]
    public void QuantityChanged_NotifiesDependentProperties()
    {
        // Arrange
        var cartProduct = new CartProduct { Quantity = 1 };
        var changedProperties = new List<string?>();
        cartProduct.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        // Act
        cartProduct.Quantity = 2;

        // Assert
        Assert.Contains(nameof(CartProduct.Quantity), changedProperties);
        Assert.Contains(nameof(CartProduct.TotalPrice), changedProperties);
        Assert.Contains(nameof(CartProduct.CanDecrease), changedProperties);
    }
}
