using ReactiveUI;

namespace MegaMarketLink.Models;

public class CartProduct : ReactiveObject
{
    private int quantity;
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity
    {
        get => quantity;
        set
        {
            this.RaiseAndSetIfChanged(ref quantity, value);
            this.RaisePropertyChanged(nameof(TotalPrice));
            this.RaisePropertyChanged(nameof(CanDecrease));
        }
    }

    public decimal TotalPrice => Price * Quantity;
    
    public bool CanDecrease => Quantity > 1;
}