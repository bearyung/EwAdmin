using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Events;
using EwAdminApp.Models;
using EwAdminApp.Services;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _contentViewModel;

    private ViewModelBase? _footerViewModel;

    private ViewModelBase? _letfSidebarViewModel;

    private LoginSettings? _loginSettings;

    // reactiveUI command for logout
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    // reactiveUI command for switching the content view model
    public ReactiveCommand<ModuleItem, Unit> SwitchContentCommand { get; }


    public MainViewModel()
    {
        ContentViewModel = new LoginViewModel();

        FooterViewModel = new ActionStatusMonitorViewModel();

        LeftSidebarViewModel = new LeftSidebarViewModel();

        // initialize the logout command
        LogoutCommand = ReactiveCommand.Create(Logout);
        
        // initialize the login command
        LoginCommand = ReactiveCommand.Create(Login);

        // Define the command for switching content
        SwitchContentCommand =
            ReactiveCommand.Create<ModuleItem>(SwitchContentViewModel, outputScheduler: RxApp.MainThreadScheduler);

        // Handle errors thrown by the command
        SwitchContentCommand.ThrownExceptions
            .Subscribe(ex =>
            {
                Console.WriteLine($"Error switching module: {ex.Message}");
                // Optionally show an error message to the user
            });

        // Listen for module change events and invoke the command
        MessageBus.Current.Listen<ModuleItemEvent>()
            .Select(x => x.ModuleItemMessage!)
            .InvokeCommand(SwitchContentCommand);

        this.WhenActivated((disposables) =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");

            // listen to the LoginEvent and update the LoginSettings property
            MessageBus.Current.Listen<LoginEvent>()
                .Subscribe(OnLoginEventReceived)
                .DisposeWith(disposables);
            
            // check for updates by DI the AppUpdateService using Locator
            // code here
            var appUpdateService = Locator.Current.GetService<IAppUpdateService>();
            appUpdateService?.DownloadUpdates();

            // listen to the ModuleItemEvent and update the ContentViewModel property
            // SwitchContentViewModel method will throw an exception if the module is not supported
            // display the error message in the console
            // MessageBus.Current.Listen<ModuleItemEvent>()
            //     .Subscribe(x => SwitchContentViewModel(x.ModuleItemMessage!))
            //     .DisposeWith(disposables);

            // when the ExecutingCommandsCount property of the ContentViewModel and FooterViewModel changes,
            // update the view model's ExecutingCommandsCount property by summing the ExecutingCommandsCount properties
            this.WhenAnyValue(x => x.ContentViewModel!.ExecutingCommandsCount)
                .CombineLatest(this.WhenAnyValue(x => x.FooterViewModel!.ExecutingCommandsCount))
                .Subscribe(x =>
                {
                    var combinedCount = x.Item1 + x.Item2;

                    // log the ExecutingCommandsCount properties
                    Console.WriteLine($"{GetType().Name}: ExecutingCommandsCount: {combinedCount}");

                    // Update the ExecutingCommandsCount property
                    ExecutingCommandsCount = combinedCount;

                    // emit the ExecutingCommandsCount property to the message bus
                    // code here
                    MessageBus.Current.SendMessage(new ExecutingCommandsCountEvent(ExecutingCommandsCount));
                })
                .DisposeWith(disposables);
            
            // add the exception handling for the LoginCommand
            // code here
            LoginCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine($"{GetType().Name}: Error logging in: {ex.Message}");
                    // Optionally show an error message to the user
                })
                .DisposeWith(disposables);
            
            // add the exception handling for the LogoutCommand
            // code here
            LogoutCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine($"{GetType().Name}: Error logging out: {ex.Message}");
                    // Optionally show an error message to the user
                })
                .DisposeWith(disposables);
            
            // add the exception handling for the SwitchContentCommand
            // code here
            SwitchContentCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine($"{GetType().Name}: Error switching module: {ex.Message}");
                    // Optionally show an error message to the user
                })
                .DisposeWith(disposables);

            // log the deactivation of viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    public ViewModelBase? ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    public ViewModelBase? FooterViewModel
    {
        get => _footerViewModel;
        set => this.RaiseAndSetIfChanged(ref _footerViewModel, value);
    }

    public ViewModelBase? LeftSidebarViewModel
    {
        get => _letfSidebarViewModel;
        set => this.RaiseAndSetIfChanged(ref _letfSidebarViewModel, value);
    }

    public LoginSettings? LoginSettings
    {
        get => _loginSettings;
        set
        {
            // if the login settings are changed, update the properties that depend on it
            this.RaiseAndSetIfChanged(ref _loginSettings, value);
            this.RaisePropertyChanged(nameof(UserName));
            this.RaisePropertyChanged(nameof(UserEmail));
            this.RaisePropertyChanged(nameof(IsLoggedIn));
        }
    }

    public string? UserName => _loginSettings?.UserName;
    public string? UserEmail => _loginSettings?.UserEmail;
    public bool IsLoggedIn => _loginSettings != null;

    private void OnLoginEventReceived(LoginEvent loginEvent)
    {
        Console.WriteLine($"OnLoginEventReceived: {loginEvent.LoginSettings?.UserName} login successfully");

        LoginSettings = loginEvent.LoginSettings;
    }

    // add an async method to logout the user
    // pop up a message control to the user to confirm the logout before really logout
    // code here
    private void Logout()
    {
        // clear the login user settings
        LoginSettings = null;

        // navigate to the login page
        ContentViewModel = new LoginViewModel(logout: true);
    }
    
    private void Login()
    {
        ContentViewModel = new LoginViewModel();
    }

    private void SwitchContentViewModel(ModuleItem moduleItem)
    {
        try
        {
            Console.WriteLine($"{GetType().Name}: ModuleItemEvent: {moduleItem.DisplayName}");

            // switch the content view model based on the selected module item
            ContentViewModel = moduleItem.Module switch
            {
                UserModuleEnum.HomeModule => new DashboardHomeViewModel(),
                UserModuleEnum.FixModule => new DashboardFixViewModel(),
                UserModuleEnum.ViewDataModule => new DashboardViewDataViewModel(),
                UserModuleEnum.ToolBoxModule => new DashboardToolBoxViewModel(),
                UserModuleEnum.SettingsModule => new SettingsViewModel(),
                UserModuleEnum.HelpModule => new HelpViewModel(),
                _ => throw new NotSupportedException($"Module {moduleItem.Module} is not supported.")
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}