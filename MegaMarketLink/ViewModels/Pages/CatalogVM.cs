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
    
    public ObservableCollection<Product> Products { get; } = new();
    
    public ReactiveCommand<Product, Unit> AddProductCommand { get; }
    public ReactiveCommand<Product, Unit> OpenProductCommand { get; set; }

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

        AddProductCommand = ReactiveCommand.Create<Product>(AddProduct);
        
        OpenProductCommand = ReactiveCommand.Create<Product>(product =>
            this.onProductSelected?.Invoke(product));
        
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
            Products.Add(product);
    }
    
    private void AddProduct(Product selectedProduct)
    {
        if (selectedProduct.Quantity <= 0)
            return;

        CartProduct? cartProduct = StaticFields.CartProducts
            .FirstOrDefault(x => x.Id == selectedProduct.Id);

        if (cartProduct != null)
        {
            if (cartProduct.Quantity >= selectedProduct.Quantity)
                return;

            cartProduct.Quantity++;
            return;
        }

        CartProduct newCartProduct = new CartProduct
        {
            Id = selectedProduct.Id,
            Name = selectedProduct.Name,
            Price = selectedProduct.Price,
            Quantity = 1
        };

        StaticFields.CartProducts.Add(newCartProduct);
    }
}