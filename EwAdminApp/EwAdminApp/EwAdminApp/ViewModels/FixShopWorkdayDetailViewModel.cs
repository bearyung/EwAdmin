using System;
using System.Reactive.Linq;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class FixShopWorkdayDetailViewModel : ViewModelBase
{
    // add a property named "SelectedShop" of type Shop
    // code here
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _shopWorkdaySelectorPanel;
    private ViewModelBase? _shopWorkdayDetailPanel;
    private ViewModelBase? _shopWorkdayDetailEditPanel;

    // add a constructor
    // when the SelectedShop property changes, console log the shop name
    // code here
    public FixShopWorkdayDetailViewModel()
    {
        // add a new instance of ShopListViewModel to the ShopPanel property
        // code here
        var shopSelectorPanelViewModel = new ShopListViewModel();

        ShopSelectorPanel = shopSelectorPanelViewModel;

        // add a new instance of ShopWorkdayListViewModel to the ShopWorkdaySelectorPanel property
        // code here
        var shopWorkdaySelectorPanelViewModel = new ShopWorkdayDetailListViewModel();
        ShopWorkdaySelectorPanel = shopWorkdaySelectorPanelViewModel;
        
        // add a new instance of ShopWorkdayDetailViewModel to the ShopWorkdayDetailPanel property
        // code here
        var shopWorkdayDetailPanelViewModel = new ShopWorkdayDetailViewModel();
        ShopWorkdayDetailPanel = shopWorkdayDetailPanelViewModel;
        
        // add a new instance of ShopWorkdayDetailEditViewModel to the ShopWorkdayDetailEditPanel property
        // code here
        var shopWorkdayDetailEditPanelViewModel = new ShopWorkdayDetailEditViewModel();
        ShopWorkdayDetailEditPanel = shopWorkdayDetailEditPanelViewModel;
    }

    public ViewModelBase? ShopSelectorPanel
    {
        get => _shopSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _shopSelectorPanel, value);
    }

    public ViewModelBase? ShopWorkdaySelectorPanel
    {
        get => _shopWorkdaySelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdaySelectorPanel, value);
    }
    
    public ViewModelBase? ShopWorkdayDetailPanel
    {
        get => _shopWorkdayDetailPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayDetailPanel, value);
    }
    
    public ViewModelBase? ShopWorkdayDetailEditPanel
    {
        get => _shopWorkdayDetailEditPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayDetailEditPanel, value);
    }

}