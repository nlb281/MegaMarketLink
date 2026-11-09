using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class CatalogVM : ViewModelBase
{
    private readonly int? categoryId;
    private readonly Action<Product>? onProductSelected;
    
    private string searchText = "";
    private bool isInCart;
    
    public ObservableCollection<CatalogProductVM> Products { get; } = new();
    
    public ReactiveCommand<CatalogProductVM, Unit> AddProductCommand { get; }
    public ReactiveCommand<CatalogProductVM, Unit> OpenProductCommand { get; set; }

    public string SearchText
    {
        get => searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref searchText, value);
            LoadProducts();
        }
    }

    public CatalogVM(int? categoryId = null, Action<Product>? onProductSelected = null)
    {
        this.categoryId = categoryId;
        this.onProductSelected = onProductSelected;

        AddProductCommand = ReactiveCommand.Create<CatalogProductVM>(AddProduct);
        
        OpenProductCommand = ReactiveCommand.Create<CatalogProductVM>(product =>
            this.onProductSelected?.Invoke(product.Product));
        
        LoadProducts();
    }

    private void LoadProducts()
    {
        Products.Clear();

        var products = StaticFields.context.Products
            .Where(x => x.IsActive
                        && x.Quantity > 0
                        && (!categoryId.HasValue || x.CategoryId == categoryId.Value)
                        && x.Name.ToLower().Contains(searchText.ToLower()))
            .OrderBy(x => x.Name)
            .ToList();

        foreach (var product in products)
            Products.Add(new CatalogProductVM(product));
    }
    
    private void AddProduct(CatalogProductVM selectedProduct)
    {
        CartProduct? cartProduct = StaticFields.CartProducts
            .FirstOrDefault(x => x.Id == selectedProduct.Product.Id);

        if (cartProduct != null)
        {
            StaticFields.CartProducts.Remove(cartProduct);
            selectedProduct.IsInCart = false;
            return;
        }
        
        if (selectedProduct.Product.Quantity <= 0)
            return;

        CartProduct newCartProduct = new CartProduct
        {
            Id = selectedProduct.Product.Id,
            Name = selectedProduct.Product.Name,
            Price = selectedProduct.Product.Price,
            Quantity = 1
        };

        StaticFields.CartProducts.Add(newCartProduct);
        selectedProduct.IsInCart = true;
    }
}