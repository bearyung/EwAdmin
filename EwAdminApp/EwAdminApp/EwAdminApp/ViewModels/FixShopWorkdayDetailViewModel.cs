using System;
using System.Reactive.Disposables;
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

        this.WhenActivated((disposables) =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // when the ExecutingCommandsCount property of the
            // ShopSelectorPanel, ShopWorkdaySelectorPanel, ShopWorkdayDetailPanel, and ShopWorkdayDetailEditPanel changes,
            // use CombineLatest to get the sum of the ExecutingCommandsCount properties
            // update the ExecutingCommandsCount property of this view model
            // code here
            this.WhenAnyValue(x => x.ShopSelectorPanel!.ExecutingCommandsCount)
                .CombineLatest(this.WhenAnyValue(x => x.ShopWorkdaySelectorPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.ShopWorkdayDetailPanel!.ExecutingCommandsCount))
                .CombineLatest(this.WhenAnyValue(x => x.ShopWorkdayDetailEditPanel!.ExecutingCommandsCount))
                .Subscribe(x =>
                {
                    var combinedCount = x.Item1.Item1.Item1 + x.Item1.Item1.Item2 + x.Item1.Item2 + x.Item2;

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