using System;
using System.Reactive;
using MegaMarketLink.Models;
using ReactiveUI;

namespace MegaMarketLink.ViewModels.Pages;


public class ProductDetailsVM : ViewModelBase
{
    private readonly Action goBack;

    public Product Product { get; }

    public ReactiveCommand<Unit, Unit> BackCommand { get; }

    public ProductDetailsVM(Product product, Action goBack)
    {
        Product = product;
        this.goBack = goBack;

        BackCommand = ReactiveCommand.Create(this.goBack);
    }
}