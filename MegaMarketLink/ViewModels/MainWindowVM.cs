using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using MegaMarketLink.ViewModels.Pages;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLink.ViewModels;

public class MainWindowVM : ViewModelBase
{
    private object? currentView;
    private Category? selectedCategory;
    public ObservableCollection<Category> Categories { get; } = new();

    public ReactiveCommand<Unit, Unit> ShowCatalogCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowCartCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowOrdersCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSellerCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAdminCommand { get; }

    public MainWindowVM()
    {
        StaticFields.CategoriesChanged += LoadCategories;
        
        ShowCatalogCommand = ReactiveCommand.Create(() =>
        {
            SelectedCategory = null;
            ShowCatalog();
        });

        ShowCartCommand = ReactiveCommand.Create(ShowCart);
        ShowOrdersCommand = ReactiveCommand.Create(ShowOrders);
        ShowSellerCommand = ReactiveCommand.Create(ShowSeller);
        ShowAdminCommand = ReactiveCommand.Create(ShowAdmin);

        LoadCategories();
        ShowCatalog();
    }

    private void LoadCategories()
    {
        Categories.Clear();

        var categories = StaticFields.context.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToList();

        foreach (var category in categories)
            Categories.Add(category);
        
        if (SelectedCategory != null && !Categories.Any(x => x.Id == SelectedCategory.Id))
        {
            SelectedCategory = null;
        }
        else
        {
            ShowCatalog();
        }
    }
    
    public object? CurrentView
    {
        get => currentView;
        set => this.RaiseAndSetIfChanged(ref currentView, value);
    }

    public bool IsSeller => StaticFields.user?.Role?.Name == "seller";

    public Category? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref selectedCategory, value);
            ShowCatalog();
        }
    }

    private void ShowCatalog()
    {
        CurrentView = new CatalogVM(SelectedCategory?.Id, ShowProductDetails);
    }
    
    private void ShowProductDetails(Product product)
    {
        var loadedProduct = StaticFields.context.Products.Include(p => p.Seller).FirstOrDefault(p => p.Id == product.Id);

        if (loadedProduct == null)
            return;
        
        CurrentView = new ProductDetailsVM(product, ShowCatalog);
    }

    private void ShowCart()
    {
        CurrentView = new CartVM();
    }

    private void ShowOrders()
    {
        CurrentView = new OrdersVM();
    }

    private void ShowSeller()
    {
        if (IsSeller)
            CurrentView = new SellerVM();
    }
    
    private void ShowAdmin()
    {
        if (IsAdmin)
            CurrentView = new AdminVM();
    }
    
    public bool IsAdmin => StaticFields.user?.Role?.Name == "admin";
}