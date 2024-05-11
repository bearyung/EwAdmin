using System;
using System.Reactive.Linq;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class FixTxPaymentViewModel : ViewModelBase
{
    // add a property named "SelectedShop" of type Shop
    // code here
    private Shop? _selectedShop;
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _shopWorkdaySelectorPanel;
    private ViewModelBase? _txSalesHeaderListPanel;
    private ViewModelBase? _txPaymentListPanel;

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
    
    public ViewModelBase? TxSalesHeaderListPanel
    {
        get => _txSalesHeaderListPanel;
        set => this.RaiseAndSetIfChanged(ref _txSalesHeaderListPanel, value);
    }
    
    public ViewModelBase? TxPaymentListPanel
    {
        get => _txPaymentListPanel;
        set => this.RaiseAndSetIfChanged(ref _txPaymentListPanel, value);
    }

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    // add a constructor
    // when the SelectedShop property changes, console log the shop name
    // code here
    public FixTxPaymentViewModel()
    {
        // add a new instance of ShopListViewModel to the ShopPanel property
        // code here
        var shopSelectorPanelViewModel = new ShopListViewModel();

        // subscribe to the SelectedShop property
        // code here
        // shopSelectorPanelViewModel.WhenAnyValue(x => x.SelectedShop)
        //     .Subscribe(shop =>
        //     {
        //         SelectedShop = shop;
        //         if (shop != null)
        //         {
        //             Console.WriteLine($"Checkpoint 2: SelectedShop changed in viewmodel: {shop.Name}");
        //         }
        //     });
        
        ShopSelectorPanel = shopSelectorPanelViewModel;
        
        // add a new instance of ShopWorkdayListViewModel to the ShopWorkdaySelectorPanel property
        // code here
        var shopWorkdaySelectorPanelViewModel = new ShopWorkdayListViewModel();
        ShopWorkdaySelectorPanel = shopWorkdaySelectorPanelViewModel;
        
        // add a new instance of TxSalesHeaderListViewModel to the TxSalesHeaderListPanel property
        // code here
        var txSalesHeaderListPanelViewModel = new TxSalesHeaderListViewModel();
        TxSalesHeaderListPanel = txSalesHeaderListPanelViewModel;
        
        // add a new instance of TxPaymentListViewModel to the TxPaymentListPanel property
        // code here
        var txPaymentListPanelViewModel = new TxPaymentListViewModel();
        TxPaymentListPanel = txPaymentListPanelViewModel;
    }
}