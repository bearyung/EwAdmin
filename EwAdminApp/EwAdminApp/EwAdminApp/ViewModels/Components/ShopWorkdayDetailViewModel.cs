using System;
using System.Reactive.Disposables;
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
        this.WhenActivated((disposables) =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            this.WhenAnyValue(x => x.SelectedShopWorkdayDetail)
                .Subscribe(shopWorkdayDetail =>
                {
                    if (shopWorkdayDetail != null)
                    {
                        Console.WriteLine($"ShopWorkdayDetail: {shopWorkdayDetail.WorkdayDetailId}");
                    }
                })
                .DisposeWith(disposables);

            // subscribe to the ShopWorkdayDetailEvent
            MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
                .Subscribe(shopWorkdayDetailEvent =>
                {
                    SelectedShopWorkdayDetail = shopWorkdayDetailEvent.ShopWorkdayDetailMessage;
                })
                .DisposeWith(disposables);
            
            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }
}