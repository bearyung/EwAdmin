using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EwAdminApp.Events;
using EwAdminApp.Models;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _contentViewModel;

    private ViewModelBase? _footerViewModel;

    private LoginSettings? _loginSettings;

    // reactiveUI command for logout
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public MainViewModel()
    {
        ContentViewModel = new LoginViewModel();

        // initialize the logout command
        LogoutCommand = ReactiveCommand.Create(Logout);

        this.WhenActivated((disposables) =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");
            
            // replace the above messageEventAggregator code with ReactiveUI message bus
            // code here
            MessageBus.Current.Listen<LoginEvent>()
                .Subscribe(OnLoginEventReceived)
                .DisposeWith(disposables);

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

        ContentViewModel = new DashboardViewModel();

        FooterViewModel = new ActionStatusMonitorViewModel();
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
}