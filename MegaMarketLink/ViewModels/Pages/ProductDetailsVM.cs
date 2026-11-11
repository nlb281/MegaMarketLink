using System;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;


public class ProductDetailsVM : ViewModelBase
{
    private bool isInCart;
    private readonly Action goBack;

    public Product Product { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }
    public ReactiveCommand<Unit, Unit> AddProductCommand { get; }


    public ProductDetailsVM(Product product, Action goBack)
    {
        Product = product;
        this.goBack = goBack;

        BackCommand = ReactiveCommand.Create(this.goBack);
        AddProductCommand = ReactiveCommand.Create(AddProduct);
        IsInCart = StaticFields.CartProducts.Any(x => x.Id == Product.Id);
    }
    
    private void AddProduct()
    {
        CartProduct? cartProduct = StaticFields.CartProducts
            .FirstOrDefault(x => x.Id == Product.Id);

        if (cartProduct != null)
        {
            StaticFields.CartProducts.Remove(cartProduct);
            IsInCart = false;
            return;
        }

        if (Product.Quantity <= 0)
            return;

        CartProduct newCartProduct = new CartProduct
        {
            Id = Product.Id,
            Name = Product.Name,
            Price = Product.Price,
            Quantity = 1
        };

        StaticFields.CartProducts.Add(newCartProduct);
        IsInCart = true;
    }
    
    public bool IsInCart
    {
        get => isInCart;
        set
        {
            this.RaiseAndSetIfChanged(ref isInCart, value);
            this.RaisePropertyChanged(nameof(AddButtonText));
        }
    }
    
    public string AddButtonText => IsInCart ? "Убрать из корзины" : "Добавить в корзину";
}