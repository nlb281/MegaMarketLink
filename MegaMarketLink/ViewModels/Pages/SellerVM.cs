using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using MegaMarketLink.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class SellerVM : ViewModelBase
{
    private string name = "";
    private string description = "";
    private string priceText = "";
    private string quantityText = "";
    private string imageUrl = "";
    private Category? selectedCategory;
    private bool isEditing;
    private int? editingProductId;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    public ReactiveCommand<Unit, Unit> AddProductCommand { get; }
    public ReactiveCommand<Product, Unit> EditProductCommand { get; }
    public ReactiveCommand<Product, Unit> DeleteProductCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }
    public ReactiveCommand<Product, Unit> RestoreProductCommand { get; }

    public bool IsEditing
    {
        get => isEditing;
        private set
        {
            this.RaiseAndSetIfChanged(ref isEditing, value);
            this.RaisePropertyChanged(nameof(SubmitButtonText));
        }
    }

    public string SubmitButtonText =>
        IsEditing ? "Сохранить изменения" : "Опубликовать товар";

    public SellerVM()
    {
        AddProductCommand = ReactiveCommand.Create(AddProduct);
        EditProductCommand = ReactiveCommand.Create<Product>(EditProduct);
        CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        DeleteProductCommand = ReactiveCommand.Create<Product>(DeleteProduct);
        RestoreProductCommand = ReactiveCommand.Create<Product>(RestoreProduct);

        LoadData();
    }

    private void LoadData()
    {
        if (StaticFields.user == null) return;

        Categories.Clear();

        var categories = StaticFields.context.Categories.Where(x => x.IsActive).OrderBy(x => x.Name).ToList();

        foreach (var category in categories)
            Categories.Add(category);

        LoadProducts();
    }

    private void LoadProducts()
    {
        if (StaticFields.user == null) return;

        Products.Clear();

        var products = StaticFields.context.Products.Include(x => x.Category).Where(x => x.SellerId == StaticFields.user.Id).OrderByDescending(x => x.CreatedAt).ToList();

        foreach (var product in products)
            Products.Add(product);
    }

    private void AddProduct()
    {
        if (StaticFields.user == null) return;

        if (string.IsNullOrWhiteSpace(Name)) return;

        if (SelectedCategory == null) return;

        if (!decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price) || price <= 0) return;

        if (!int.TryParse(QuantityText, out int quantity) || quantity < 0) return;

        if (IsEditing && editingProductId.HasValue)
        {
            var product = StaticFields.context.Products.FirstOrDefault(x => x.Id == editingProductId.Value && x.SellerId == StaticFields.user.Id);

            if (product == null) return;

            UpdateProduct(product, price, quantity);
        }
        else
        {
            var product = new Product
            {
                SellerId = StaticFields.user.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            UpdateProduct(product, price, quantity);

            StaticFields.context.Products.Add(product);
        }

        StaticFields.context.SaveChanges();

        LoadProducts();
        ClearForm();
    }

    private void UpdateProduct(Product product, decimal price, int quantity)
    {
        product.Name = Name.Trim();
        product.Description = Description.Trim();
        product.Price = price;
        product.Quantity = quantity;
        product.ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim();

        product.CategoryId = SelectedCategory!.Id;
        product.Category = SelectedCategory;
    }

    private void EditProduct(Product product)
    {
        if (product == null) return;

        Name = product.Name;
        Description = product.Description ?? "";
        PriceText = product.Price.ToString(CultureInfo.CurrentCulture);
        QuantityText = product.Quantity.ToString();
        ImageUrl = product.ImageUrl ?? "";

        SelectedCategory = Categories.FirstOrDefault(x => x.Id == product.CategoryId);

        editingProductId = product.Id;
        IsEditing = true;
    }

    
    private void DeleteProduct(Product product)
    {
        if (StaticFields.user == null || product == null) return;

        var dbProduct = StaticFields.context.Products.FirstOrDefault(x => x.Id == product.Id && x.SellerId == StaticFields.user.Id);

        if (dbProduct == null) return;

        dbProduct.IsActive = false;

        StaticFields.context.SaveChanges();

        LoadProducts();
    }

    private void RestoreProduct(Product product)
    {
        if (StaticFields.user == null || product == null) return;

        var dbProduct = StaticFields.context.Products.FirstOrDefault(x => x.Id == product.Id && x.SellerId == StaticFields.user.Id);

        if (dbProduct == null) return;

        dbProduct.IsActive = true;

        StaticFields.context.SaveChanges();

        LoadProducts();
    }
    

    private void CancelEdit()
    {
        ClearForm();
    }

    private void ClearForm()
    {
        Name = "";
        Description = "";
        PriceText = "";
        QuantityText = "";
        ImageUrl = "";
        SelectedCategory = null;

        editingProductId = null;
        IsEditing = false;
    }

    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public string Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }

    public string PriceText
    {
        get => priceText;
        set => this.RaiseAndSetIfChanged(ref priceText, value);
    }

    public string QuantityText
    {
        get => quantityText;
        set => this.RaiseAndSetIfChanged(ref quantityText, value);
    }

    public string ImageUrl
    {
        get => imageUrl;
        set => this.RaiseAndSetIfChanged(ref imageUrl, value);
    }

    public Category? SelectedCategory
    {
        get => selectedCategory;
        set => this.RaiseAndSetIfChanged(ref selectedCategory, value);
    }
}