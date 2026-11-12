using MegaMarketLink.Models;
using MegaMarketLink.ViewModels.Pages;

namespace MegaMarketLink.Tests;

public class ProductDetailsVMTests : IDisposable
{
    public ProductDetailsVMTests()
    {
        StaticFields.CartProducts.Clear();
    }

    public void Dispose()
    {
        StaticFields.CartProducts.Clear();
    }

    [Fact]
    public void Constructor_WhenProductIsAlreadyInCart_SetsCartState()
    {
        // Arrange
        var product = CreateProduct();
        StaticFields.CartProducts.Add(new CartProduct { Id = product.Id, Quantity = 1 });

        // Act
        var viewModel = new ProductDetailsVM(product, () => { });

        // Assert
        Assert.True(viewModel.IsInCart);
        Assert.Equal("Убрать из корзины", viewModel.AddButtonText);
    }

    [Fact]
    public void AddProductCommand_ForAvailableProduct_AddsProductToCart()
    {
        // Arrange
        var product = CreateProduct(quantity: 5, price: 350m);
        var viewModel = new ProductDetailsVM(product, () => { });

        // Act
        viewModel.AddProductCommand.Execute().Subscribe();

        // Assert
        var cartProduct = Assert.Single(StaticFields.CartProducts);
        Assert.True(viewModel.IsInCart);
        Assert.Equal(product.Id, cartProduct.Id);
        Assert.Equal(product.Name, cartProduct.Name);
        Assert.Equal(product.Price, cartProduct.Price);
        Assert.Equal(1, cartProduct.Quantity);
    }

    [Fact]
    public void AddProductCommand_ForProductAlreadyInCart_RemovesIt()
    {
        // Arrange
        var product = CreateProduct();
        var viewModel = new ProductDetailsVM(product, () => { });
        viewModel.AddProductCommand.Execute().Subscribe();

        // Act
        viewModel.AddProductCommand.Execute().Subscribe();

        // Assert
        Assert.False(viewModel.IsInCart);
        Assert.Empty(StaticFields.CartProducts);
    }

    [Fact]
    public void AddProductCommand_ForOutOfStockProduct_DoesNotAddIt()
    {
        // Arrange
        var viewModel = new ProductDetailsVM(CreateProduct(quantity: 0), () => { });

        // Act
        viewModel.AddProductCommand.Execute().Subscribe();

        // Assert
        Assert.False(viewModel.IsInCart);
        Assert.Empty(StaticFields.CartProducts);
    }

    [Fact]
    public void BackCommand_InvokesProvidedCallback()
    {
        // Arrange
        var wasCalled = false;
        var viewModel = new ProductDetailsVM(CreateProduct(), () => wasCalled = true);

        // Act
        viewModel.BackCommand.Execute().Subscribe();

        // Assert
        Assert.True(wasCalled);
    }

    private static Product CreateProduct(int quantity = 1, decimal price = 100m) => new()
    {
        Id = 42,
        Name = "Тестовый товар",
        Price = price,
        Quantity = quantity
    };
}
