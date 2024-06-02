using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.FixPages;

public class FixTxPaymentViewModel : ViewModelBase
{
    // add a property named "SelectedShop" of type Shop
    // code here
    private Shop? _selectedShop;
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _shopWorkdaySelectorPanel;
    private ViewModelBase? _txPaymentDetailPanel;
    private ViewModelBase? _txPaymentListPanel;
    private ViewModelBase? _txSalesHeaderListPanel;
    private ViewModelBase? _txPaymentDetailEditPanel;

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
        var shopWorkdaySelectorPanelViewModel = new ShopWorkdayDetailListViewModel();
        ShopWorkdaySelectorPanel = shopWorkdaySelectorPanelViewModel;

        // add a new instance of TxSalesHeaderListViewModel to the TxSalesHeaderListPanel property
        // code here
        var txSalesHeaderListPanelViewModel = new TxSalesHeaderListViewModel();
        TxSalesHeaderListPanel = txSalesHeaderListPanelViewModel;

        // add a new instance of TxPaymentListViewModel to the TxPaymentListPanel property
        // code here
        var txPaymentListPanelViewModel = new TxPaymentListViewModel();
        TxPaymentListPanel = txPaymentListPanelViewModel;
        
        // add a new instance of TxPaymentDetailViewModel to the TxPaymentDetailPanel property
        // code here
        var txPaymentDetailPanelViewModel = new TxPaymentDetailViewModel();
        TxPaymentDetailPanel = txPaymentDetailPanelViewModel;
        
        // add a new instance of TxPaymentDetailEditViewModel to the TxPaymentDetailEditPanel property
        // code here
        var txPaymentDetailEditPanelViewModel = new TxPaymentDetailEditViewModel();
        TxPaymentDetailEditPanel = txPaymentDetailEditPanelViewModel;
        
        this.WhenActivated((disposables) =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // when the ExecutingCommandsCount property of the
            // ShopSelectorPanel, ShopWorkdaySelectorPanel, TxSalesHeaderListPanel, TxPaymentListPanel, TxPaymentDetailPanel, and TxPaymentDetailEditPanel changes,
            // use CombineLatest to get the sum of the ExecutingCommandsCount properties
            // update the ExecutingCommandsCount property of this view model
            // code here
            this.WhenAnyValue(x => x.ShopSelectorPanel!.ExecutingCommandsCount)
                .CombineLatest(this.WhenAnyValue(x => x.ShopWorkdaySelectorPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.TxSalesHeaderListPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.TxPaymentListPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.TxPaymentDetailPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.TxPaymentDetailEditPanel!.ExecutingCommandsCount))
                .Subscribe(x =>
                {
                    var combinedCount = x.Item1.Item1.Item1.Item1.Item1 + x.Item1.Item1.Item1.Item1.Item2 + x.Item1.Item1.Item1.Item2 + x.Item1.Item1.Item2 + x.Item1.Item2 + x.Item2;

                    // log the ExecutingCommandsCount properties
                    Console.WriteLine($"{GetType().Name}: ExecutingCommandsCount: {combinedCount}");

                    // Update the ExecutingCommandsCount property
                    ExecutingCommandsCount = combinedCount;
                })
                .DisposeWith(disposables);
            
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
    
    public ViewModelBase? TxPaymentDetailPanel
    {
        get => _txPaymentDetailPanel;
        set => this.RaiseAndSetIfChanged(ref _txPaymentDetailPanel, value);
    }
    
    public ViewModelBase? TxPaymentDetailEditPanel
    {
        get => _txPaymentDetailEditPanel;
        set => this.RaiseAndSetIfChanged(ref _txPaymentDetailEditPanel, value);
    }
}