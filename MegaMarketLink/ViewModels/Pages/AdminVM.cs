using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class AdminVM : ViewModelBase
{
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    private string categoryName = "";
    private Category? editingCategory;
    private bool isEditingCategory;

    public string CategoryName
    {
        get => categoryName;
        set => this.RaiseAndSetIfChanged(ref categoryName, value);
    }

    public bool IsEditingCategory
    {
        get => isEditingCategory;
        set => this.RaiseAndSetIfChanged(ref isEditingCategory, value);
    }

    public ReactiveCommand<User, Unit> BanUserCommand { get; }
    public ReactiveCommand<User, Unit> UnbanUserCommand { get; }

    public ReactiveCommand<Product, Unit> ToggleProductCommand { get; }

    public ReactiveCommand<Category, Unit> EditCategoryCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCategoryCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCategoryEditCommand { get; }
    public ReactiveCommand<Category, Unit> ToggleCategoryCommand { get; }

    public AdminVM()
    {
        BanUserCommand = ReactiveCommand.Create<User>(BanUser);
        UnbanUserCommand = ReactiveCommand.Create<User>(UnbanUser);

        ToggleProductCommand = ReactiveCommand.Create<Product>(ToggleProduct);

        EditCategoryCommand = ReactiveCommand.Create<Category>(EditCategory);
        SaveCategoryCommand = ReactiveCommand.Create(SaveCategory);
        CancelCategoryEditCommand = ReactiveCommand.Create(CancelCategoryEdit);
        ToggleCategoryCommand = ReactiveCommand.Create<Category>(ToggleCategory);

        LoadData();
    }

    private bool IsAdmin => StaticFields.user?.Role?.Name == "admin";

    private void LoadData()
    {
        if (!IsAdmin) return;

        Users.Clear();
        Products.Clear();
        Categories.Clear();

        foreach (var user in StaticFields.context.Users.Include(x => x.Role).OrderBy(x => x.Id).ToList())
            Users.Add(user);

        foreach (var product in StaticFields.context.Products.Include(x => x.Category).Include(x => x.Seller).OrderByDescending(x => x.CreatedAt).ToList())
            Products.Add(product);

        foreach (var category in StaticFields.context.Categories.OrderBy(x => x.Name).ToList())
            Categories.Add(category);
    }

    private void BanUser(User user)
    {
        if (!IsAdmin || user == null) return;

        if (user.Id == StaticFields.user!.Id || user.Role?.Name == "admin") return;

        user.IsBanned = true;
        StaticFields.context.SaveChanges();

        RefreshUser(user);
    }

    private void UnbanUser(User user)
    {
        if (!IsAdmin || user == null)
            return;

        user.IsBanned = false;
        StaticFields.context.SaveChanges();

        RefreshUser(user);
    }
    
    private void RefreshUser(User user)
    {
        int index = Users.IndexOf(user);

        if (index < 0) return;
        
        Users[index] = user;
    }
    
    private void ToggleProduct(Product product)
    {
        if (!IsAdmin || product == null) return;

        product.IsActive = !product.IsActive;
        StaticFields.context.SaveChanges();

        int index = Products.IndexOf(product);

        if (index >= 0)
            Products[index] = product;
    }
    
    private void EditCategory(Category category)
    {
        if (!IsAdmin || category == null) return;

        editingCategory = category;
        CategoryName = category.Name;
        IsEditingCategory = true;
    }
    
    private void SaveCategory()
    {
        if (!IsAdmin || editingCategory == null || string.IsNullOrWhiteSpace(CategoryName)) return;

        var newName = CategoryName.Trim();
        
        bool nameExists = StaticFields.context.Categories.Any(x => x.Id != editingCategory.Id && x.Name.ToLower() == newName.ToLower());

        if (nameExists) return;

        editingCategory.Name = newName;

        StaticFields.context.SaveChanges();
        StaticFields.NotifyCategoriesChanged();

        CancelCategoryEdit();
        
        var categories = Categories.OrderBy(x => x.Name).ToList();
        Categories.Clear();

        foreach (var category in categories)
            Categories.Add(category);
    }
    
    private void ToggleCategory(Category category)
    {
        if (!IsAdmin || category == null) return;

        category.IsActive = !category.IsActive;
        StaticFields.context.SaveChanges();

        int index = Categories.IndexOf(category);

        if (index >= 0)
            Categories[index] = category;
        
        StaticFields.NotifyCategoriesChanged();
    }

    private void CancelCategoryEdit()
    {
        editingCategory = null;
        CategoryName = "";
        IsEditingCategory = false;
    }
}