using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _contentViewModel;

    private LoginSettings? _loginSettings;

    // reactiveUI command for logout
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public MainViewModel()
    {
        ContentViewModel = new LoginViewModel();

        // replace the above messageEventAggregator code with ReactiveUI message bus
        // code here
        MessageBus.Current.Listen<LoginEvent>()
            .Subscribe(OnLoginEventReceived);

        // initialize the logout command
        LogoutCommand = ReactiveCommand.Create(Logout);
    }

    public ViewModelBase? ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
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