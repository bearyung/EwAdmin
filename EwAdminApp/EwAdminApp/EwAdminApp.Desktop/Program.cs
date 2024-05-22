using System;
using Avalonia;
using Avalonia.ReactiveUI;
using EwAdminApp.Desktop.Services;
using EwAdminApp.Services;
using Splat;
using Velopack;

namespace EwAdminApp.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // add the AppUpdateService to the Locator
        // code here
        Locator.CurrentMutable.RegisterConstant(new AppUpdateService(), typeof(IAppUpdateService));
        
        
        VelopackApp.Build().Run();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}