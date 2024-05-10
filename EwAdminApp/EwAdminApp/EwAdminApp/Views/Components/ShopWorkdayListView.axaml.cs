using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayListView : UserControl
{
    public static readonly StyledProperty<Shop?> SelectedShopProperty = 
        AvaloniaProperty.Register<ShopListView, Shop?>(nameof(SelectedShop));
    public ShopWorkdayListView()
    {
        InitializeComponent();
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
        
        // add the data context
        DataContext = new ShopWorkdayListViewModel();
    }
    
    // Add a property named "SelectedShop" of type Shop
    public Shop? SelectedShop
    {
        get => GetValue(SelectedShopProperty);
        set => SetValue(SelectedShopProperty, value);
    }
}