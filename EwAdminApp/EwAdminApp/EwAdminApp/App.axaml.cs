using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EwAdminApp.ViewModels;
using EwAdminApp.Views;
using Splat;

namespace EwAdminApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // register a HttpClient
        // this will be used to make HTTP requests throughout the whole app and will be injected into the view models
        // this is a singleton service
        Locator.CurrentMutable.RegisterConstant(new System.Net.Http.HttpClient(), typeof(System.Net.Http.HttpClient));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}