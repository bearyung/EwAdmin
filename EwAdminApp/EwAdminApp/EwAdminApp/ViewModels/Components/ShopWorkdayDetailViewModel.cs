using System;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class ShopWorkdayDetailViewModel : ViewModelBase
{
    // this viewmodel is used to display the details of a ShopWorkdayDetail which received from MessageBus
    // add a property named "ShopWorkdayDetail" of type ShopWorkdayDetail
    // code here
    private ShopWorkdayDetail? _selectedShopWorkdayDetail;

    public ShopWorkdayDetail? SelectedShopWorkdayDetail
    {
        get => _selectedShopWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayDetail, value);
    }

    // add a constructor
    // when the ShopWorkdayDetail property changes, console log the shop workday detail
    // use MessageBus.Current.Listen<ShopWorkdayDetailEvent>() to subscribe to the event, and set the ShopWorkdayDetail property
    // code here
    public ShopWorkdayDetailViewModel()
    {
        this.WhenAnyValue(x => x.SelectedShopWorkdayDetail)
            .Subscribe(shopWorkdayDetail =>
            {
                if (shopWorkdayDetail != null)
                {
                    Console.WriteLine($"ShopWorkdayDetail: {shopWorkdayDetail}");
                }
            });
        
        // subscribe to the ShopWorkdayDetailEvent
        MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
            .Subscribe(shopWorkdayDetailEvent =>
            {
                SelectedShopWorkdayDetail = shopWorkdayDetailEvent.ShopWorkdayDetailMessage;
            });
    }
}