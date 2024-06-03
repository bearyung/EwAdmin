using System;
using System.Reactive.Disposables;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.FixPages;

public class FixTxSalesHeaderCusCountViewModel : ViewModelBase
{
    // add a property named "SelectedShop" of type Shop
    // code here
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _shopWorkdaySelectorPanel;
    private ViewModelBase? _txSalesHeaderListPanel;
    private ViewModelBase? _txSalesHeaderDetailPanel;
    private ViewModelBase? _txSalesHeaderCusCountEditPanel;

    // add a constructor
    // when the SelectedShop property changes, console log the shop name
    // code here
    public FixTxSalesHeaderCusCountViewModel()
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
        var shopWorkdaySelectorPanelViewModel = new ShopWorkdayDetailListViewModel();
        ShopWorkdaySelectorPanel = shopWorkdaySelectorPanelViewModel;

        // add a new instance of TxSalesHeaderListViewModel to the TxSalesHeaderListPanel property
        var txSalesHeaderListPanelViewModel = new TxSalesHeaderListViewModel();
        TxSalesHeaderListPanel = txSalesHeaderListPanelViewModel;
        
        // add a new instance of TxSalesHeaderDetailViewModel to the TxSalesHeaderDetailPanel property
        var txSalesHeaderDetailPanelViewModel = new TxSalesHeaderDetailViewModel();
        TxSalesHeaderDetailPanel = txSalesHeaderDetailPanelViewModel;

        // add a new instance of TxSalesHeaderDetailEditTableViewModel to the TxSalesHeaderDetailEditTablePanel property
        var txSalesHeaderDetailEditCusCountViewModel = new TxSalesHeaderDetailEditCusCountViewModel();
        TxSalesHeaderCusCountEditPanel = txSalesHeaderDetailEditCusCountViewModel;
        
        this.WhenActivated((disposables) =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
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

    public ViewModelBase? TxSalesHeaderListPanel
    {
        get => _txSalesHeaderListPanel;
        set => this.RaiseAndSetIfChanged(ref _txSalesHeaderListPanel, value);
    }

    public ViewModelBase? TxSalesHeaderCusCountEditPanel
    {
        get => _txSalesHeaderCusCountEditPanel;
        set => this.RaiseAndSetIfChanged(ref _txSalesHeaderCusCountEditPanel, value);
    }
    
    public ViewModelBase? TxSalesHeaderDetailPanel
    {
        get => _txSalesHeaderDetailPanel;
        set => this.RaiseAndSetIfChanged(ref _txSalesHeaderDetailPanel, value);
    }
}