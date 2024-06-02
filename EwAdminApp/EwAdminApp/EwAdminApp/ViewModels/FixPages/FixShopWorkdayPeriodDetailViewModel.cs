using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.FixPages;

public class FixShopWorkdayPeriodDetailViewModel : ViewModelBase
{
    // add a property named "ShopSelectorPanel" of type ViewModelBase
    // code here
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _shopWorkdaySelectorPanel;
    private ViewModelBase? _shopWorkdayPeriodSelectorPanel;
    private ViewModelBase? _shopWorkdayPeriodDetailPanel;
    private ViewModelBase? _shopWorkdayPeriodDetailEditPanel;
    
    // add a constructor
    // code here
    public FixShopWorkdayPeriodDetailViewModel()
    {
        // add a new instance of ShopListViewModel to the ShopSelectorPanel property
        // code here
        var shopSelectorPanelViewModel = new ShopListViewModel();
        ShopSelectorPanel = shopSelectorPanelViewModel;

        // add a new instance of ShopWorkdayListViewModel to the ShopWorkdaySelectorPanel property
        // code here
        var shopWorkdaySelectorPanelViewModel = new ShopWorkdayDetailListViewModel();
        ShopWorkdaySelectorPanel = shopWorkdaySelectorPanelViewModel;
        
        // add a new instance of ShopWorkdayPeriodListViewModel to the ShopWorkdayPeriodSelectorPanel property
        // code here
        var shopWorkdayPeriodSelectorPanelViewModel = new ShopWorkdayPeriodDetailListViewModel();
        ShopWorkdayPeriodSelectorPanel = shopWorkdayPeriodSelectorPanelViewModel;
        
        // add a new instance of ShopWorkdayPeriodDetailViewModel to the ShopWorkdayPeriodDetailPanel property
        // code here
        var shopWorkdayPeriodDetailPanelViewModel = new ShopWorkdayPeriodDetailViewModel();
        ShopWorkdayPeriodDetailPanel = shopWorkdayPeriodDetailPanelViewModel;
        
        // add a new instance of ShopWorkdayPeriodDetailEditViewModel to the ShopWorkdayPeriodDetailEditPanel property
        // code here
        var shopWorkdayPeriodDetailEditPanelViewModel = new ShopWorkdayPeriodDetailEditViewModel();
        ShopWorkdayPeriodDetailEditPanel = shopWorkdayPeriodDetailEditPanelViewModel;
        
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
    
    public ViewModelBase? ShopWorkdayPeriodSelectorPanel
    {
        get => _shopWorkdayPeriodSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayPeriodSelectorPanel, value);
    }
    
    public ViewModelBase? ShopWorkdayPeriodDetailPanel
    {
        get => _shopWorkdayPeriodDetailPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayPeriodDetailPanel, value);
    }
    
    public ViewModelBase? ShopWorkdayPeriodDetailEditPanel
    {
        get => _shopWorkdayPeriodDetailEditPanel;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayPeriodDetailEditPanel, value);
    }
}