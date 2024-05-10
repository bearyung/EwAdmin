using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;
using ReactiveUI;

namespace EwAdminApp.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }
}