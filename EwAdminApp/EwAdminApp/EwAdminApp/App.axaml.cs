using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EwAdminApp.Events;
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
        // this HttpClient will be used to make API calls
        // set the base address of the HttpClient to https://ewadminapi.azurewebsites.net
        // Register the HttpClient as a singleton using Locator.CurrentMutable
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://ewadminapi.azurewebsites.net")
            
            // set the base address of the HttpClient to  for local development
            // BaseAddress = new Uri("")
        };
        
        Locator.CurrentMutable.RegisterConstant(httpClient, typeof(HttpClient));

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