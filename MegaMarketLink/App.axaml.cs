using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MegaMarketLink.ViewModels;
using MegaMarketLink.Views;

namespace MegaMarketLink;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Auth();
            // {
            //     DataContext = new MainWindowViewModel(),
            // };
        }

        base.OnFrameworkInitializationCompleted();
    }
}