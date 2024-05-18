using Avalonia.Controls;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayDetailView : ReactiveUserControl<ShopWorkdayDetailViewModel>
{
    public ShopWorkdayDetailView()
    {
        InitializeComponent();
    }
}