using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MegaMarketLink.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class CartVM : ViewModelBase
{
    private string deliveryAddress = "";
    private string message = "";
    public ObservableCollection<CartProduct> CartProducts => StaticFields.CartProducts;

    public decimal TotalPrice => CartProducts.Sum(x => x.TotalPrice);
    public bool IsEmpty => CartProducts.Count == 0;

    public ReactiveCommand<CartProduct, Unit> AddProductCommand { get; }
    public ReactiveCommand<CartProduct, Unit> RemoveProductCommand { get; }
    public ReactiveCommand<CartProduct, Unit> DeleteProductCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateOrderCommand { get; }

    public CartVM()
    {
        AddProductCommand = ReactiveCommand.Create<CartProduct>(AddProduct);
        RemoveProductCommand = ReactiveCommand.Create<CartProduct>(RemoveProduct);
        DeleteProductCommand = ReactiveCommand.Create<CartProduct>(DeleteProduct);
        CreateOrderCommand = ReactiveCommand.CreateFromTask(CreateOrder);
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
    
    private async Task CreateOrder()
    {
        if (StaticFields.user == null)
            return;

        if (CartProducts.Count == 0)
        {
            Message = "Корзина пуста";
            return;
        }

        if (string.IsNullOrWhiteSpace(DeliveryAddress))
        {
            Message = "Укажите адрес доставки";
            return;
        }

        await using var transaction = await StaticFields.context.Database.BeginTransactionAsync();

        try
        {
            var cartItems = CartProducts.ToList();

            var productIds = cartItems.Select(x => x.Id).ToList();

            var products = await StaticFields.context.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();
            
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(x => x.Id == item.Id);

                if (product == null || !product.IsActive)
                {
                    Message = $"Товар «{item.Name}» больше недоступен";
                    await transaction.RollbackAsync();
                    return;
                }

                if (product.Quantity < item.Quantity)
                {
                    Message = $"Недостаточно товара «{product.Name}» на складе";
                    await transaction.RollbackAsync();
                    return;
                }
            }
            
            var status = await StaticFields.context.OrderStatuses.FirstOrDefaultAsync(x => x.Name == "Новый");
            
            var order = new Order
            {
                UserId = StaticFields.user.Id,
                OrderDate = DateTime.UtcNow,
                DeliveryAddress = DeliveryAddress.Trim(),
                TotalPrice = TotalPrice,
                StatusId = status.Id
            };
            
            foreach (var item in cartItems)
            {
                var product = products.First(x => x.Id == item.Id);

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
                
                product.Quantity -= item.Quantity;
            }

            StaticFields.context.Orders.Add(order);
            
            var cart = await StaticFields.context.Carts.FirstOrDefaultAsync(x => x.UserId == StaticFields.user.Id);

            if (cart != null)
            {
                var dbCartItems = await StaticFields.context.CartItems.Where(x => x.CartId == cart.Id).ToListAsync();
                StaticFields.context.CartItems.RemoveRange(dbCartItems);
            }

            await StaticFields.context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            CartProducts.Clear();

            DeliveryAddress = "";
            Message = "Заказ успешно оформлен!";

            this.RaisePropertyChanged(nameof(TotalPrice));
            this.RaisePropertyChanged(nameof(IsEmpty));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Message = $"Ошибка оформления заказа: {ex.Message}";
        }
    }
    
    public string DeliveryAddress
    {
        get => deliveryAddress;
        set => this.RaiseAndSetIfChanged(ref deliveryAddress, value);
    }

    public string Message
    {
        get => message;
        set => this.RaiseAndSetIfChanged(ref message, value);
    }
}