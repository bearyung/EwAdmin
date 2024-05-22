using Avalonia.Controls;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;

namespace EwAdminApp.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}