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
}