using System.Collections.ObjectModel;

namespace MegaMarketLink.Models;

public class StaticFields
{
    public static MegaMarketLinkContext context = new MegaMarketLinkContext();
    public static User? user;
    public static ObservableCollection<CartProduct> CartProducts { get; } = new();
}