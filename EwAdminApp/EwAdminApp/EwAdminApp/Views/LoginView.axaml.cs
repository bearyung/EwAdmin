using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;
using ReactiveUI;

namespace EwAdminApp.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();
        
        this.WhenActivated((disposables) =>
        {
            ApiKeyTextInput.Focus();
        });
    }
}