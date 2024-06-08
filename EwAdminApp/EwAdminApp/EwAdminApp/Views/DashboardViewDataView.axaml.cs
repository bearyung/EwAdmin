using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;

namespace EwAdminApp.Views;

public partial class DashboardViewDataView : ReactiveUserControl<DashboardViewDataViewModel>
{
    public DashboardViewDataView()
    {
        InitializeComponent();
    }
}