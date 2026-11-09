using System;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;


public class ProductDetailsVM : ViewModelBase
{
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
    }
    
    private void AddProduct()
    {
        if (Product.Quantity <= 0)
            return;

        CartProduct? cartProduct = StaticFields.CartProducts
            .FirstOrDefault(x => x.Id == Product.Id);

        if (cartProduct != null)
        {
            if (cartProduct.Quantity >= Product.Quantity)
                return;

            cartProduct.Quantity++;
            return;
        }

        CartProduct newCartProduct = new CartProduct
        {
            Id = Product.Id,
            Name = Product.Name,
            Price = Product.Price,
            Quantity = 1
        };

        StaticFields.CartProducts.Add(newCartProduct);
    }
}