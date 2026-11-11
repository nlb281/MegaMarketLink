using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MegaMarketLink.Models;

public class StaticFields
{
    public static MegaMarketLinkContext context = new MegaMarketLinkContext();
    public static User? user;
    public static ObservableCollection<CartProduct> CartProducts { get; } = new();
    
    public static async Task LoadCart()
    {
        if (user == null)
            return;

        CartProducts.Clear();

        var cart = await  context.Carts.FirstOrDefaultAsync(x => x.UserId == user.Id);

        if (cart == null)
            return;

        var items = await  context.CartItems.Where(x => x.CartId == cart.Id).Select(x => new
            {x.Product.Id, x.Product.Name, x.Product.Price, x.Quantity}).ToListAsync();

        foreach (var item in items)
        {
            CartProducts.Add(new CartProduct
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Quantity = item.Quantity
            });
        }
    }

    public static void SyncCart()
    {
        if (user == null)
            return;

        var cart = context.Carts.FirstOrDefault(x => x.UserId == user.Id);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Carts.Add(cart);
            context.SaveChanges();
        }

        var dbItems = context.CartItems.Where(x => x.CartId == cart.Id).ToList();
        
        foreach (var dbItem in dbItems)
        {
            if (!CartProducts.Any(x => x.Id == dbItem.ProductId)) context.CartItems.Remove(dbItem);
        }

        foreach (var product in CartProducts)
        {
            var dbItem = dbItems.FirstOrDefault(x => x.ProductId == product.Id);

            if (dbItem == null)
            {
                context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = product.Quantity
                });
            }
            else
            {
                dbItem.Quantity = product.Quantity;
            }
        }

        context.SaveChanges();
    }
}