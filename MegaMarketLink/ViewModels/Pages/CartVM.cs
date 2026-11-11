using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class CartVM : ViewModelBase
{
    public ObservableCollection<CartProduct> CartProducts => StaticFields.CartProducts;

    public decimal TotalPrice => CartProducts.Sum(x => x.TotalPrice);
    public bool IsEmpty => CartProducts.Count == 0;

    public ReactiveCommand<CartProduct, Unit> AddProductCommand { get; }
    public ReactiveCommand<CartProduct, Unit> RemoveProductCommand { get; }
    public ReactiveCommand<CartProduct, Unit> DeleteProductCommand { get; }

    public CartVM()
    {
        AddProductCommand = ReactiveCommand.Create<CartProduct>(AddProduct);
        RemoveProductCommand = ReactiveCommand.Create<CartProduct>(RemoveProduct);
        DeleteProductCommand = ReactiveCommand.Create<CartProduct>(DeleteProduct);
    }

    private void AddProduct(CartProduct cartProduct)
    {
        var product = StaticFields.context.Products.FirstOrDefault(x => x.Id == cartProduct.Id);

        if (product == null || !product.IsActive)
            return;

        if (cartProduct.Quantity >= product.Quantity)
            return;

        cartProduct.Quantity++;
        this.RaisePropertyChanged(nameof(TotalPrice));
        
        StaticFields.SyncCart();
    }

    private void RemoveProduct(CartProduct cartProduct)
    {
        if (cartProduct.Quantity <= 1)
            return;

        cartProduct.Quantity--;
        this.RaisePropertyChanged(nameof(TotalPrice));
        
        StaticFields.SyncCart();
    }

    private void DeleteProduct(CartProduct cartProduct)
    {
        CartProducts.Remove(cartProduct);
        StaticFields.SyncCart();
        this.RaisePropertyChanged(nameof(TotalPrice));
        this.RaisePropertyChanged(nameof(IsEmpty));
    }
}