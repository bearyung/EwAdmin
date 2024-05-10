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

    public ViewModelBase? ShopSelectorPanel
    {
        get => _shopSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _shopSelectorPanel, value);
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
        var viewModel = new ShopListViewModel();

        // subscribe to the SelectedShop property
        // code here
        viewModel.WhenAnyValue(x => x.SelectedShop)
            .Subscribe(shop =>
            {
                SelectedShop = shop;
                if (shop != null)
                {
                    Console.WriteLine($"Checkpoint 2: SelectedShop changed in viewmodel: {shop.Name}");
                }
            });

        ShopSelectorPanel = viewModel;
    }
}