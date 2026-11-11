using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MegaMarketLink.Models;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;

public class OrdersVM : ViewModelBase
{
    private bool isEmpty = true;
    private string message = "";
    public ObservableCollection<Order> Orders { get; } = new();
    
    public OrdersVM()
    {
        _ = LoadOrders();
    }

    private async Task LoadOrders()
    {
        if (StaticFields.user == null)
            return;

        try
        {
            var orders = await StaticFields.context.Orders
                .AsNoTracking()
                .Include(x => x.Status)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .Where(x => x.UserId == StaticFields.user.Id)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            Orders.Clear();

            foreach (var order in orders)
                Orders.Add(order);

            IsEmpty = Orders.Count == 0;
        }
        catch (Exception ex)
        {
            Message = $"Ошибка загрузки заказов: {ex.Message}";
        }
    }
    
    public bool IsEmpty
    {
        get => isEmpty;
        set => this.RaiseAndSetIfChanged(ref isEmpty, value);
    }
    
       
    public string Message 
    {
        get => message;
        set => this.RaiseAndSetIfChanged(ref message, value);
    }
}