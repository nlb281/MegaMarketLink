using System.Linq;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class CatalogProductVM : ViewModelBase
{
    private bool isInCart;

    public Product Product { get; }

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

    public CatalogProductVM(Product product)
    {
        Product = product;

        IsInCart = StaticFields.CartProducts.Any(x => x.Id == product.Id);
    }
}