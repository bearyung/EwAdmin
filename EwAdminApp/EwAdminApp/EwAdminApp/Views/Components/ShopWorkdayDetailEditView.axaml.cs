using Avalonia.Controls;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayDetailEditView : ReactiveUserControl<ShopWorkdayDetailEditViewModel>
{
    public ShopWorkdayDetailEditView()
    {
        InitializeComponent();
    }
}