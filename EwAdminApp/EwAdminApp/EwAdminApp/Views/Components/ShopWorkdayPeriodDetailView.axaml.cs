using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayPeriodDetailView : ReactiveUserControl<ShopWorkdayPeriodDetailViewModel>
{
    public ShopWorkdayPeriodDetailView()
    {
        InitializeComponent();
    }
}