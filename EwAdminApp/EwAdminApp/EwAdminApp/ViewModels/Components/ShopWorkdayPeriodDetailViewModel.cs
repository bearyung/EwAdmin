using System;
using System.Reactive.Disposables;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class ShopWorkdayPeriodDetailViewModel : ViewModelBase
{
    // add a property named "ShopWorkdayPeriodDetail" of type ShopWorkdayPeriodDetail
    // code here
    private ShopWorkdayPeriodDetail? _selectedShopWorkdayPeriodDetail;

    public ShopWorkdayPeriodDetail? SelectedShopWorkdayPeriodDetail
    {
        get => _selectedShopWorkdayPeriodDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayPeriodDetail, value);
    }

    // add a constructor
    // when the ShopWorkdayPeriodDetail property changes, console log the shop workday period detail
    // code here
    public ShopWorkdayPeriodDetailViewModel()
    {
        this.WhenActivated((disposable) =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            this.WhenAnyValue(x => x.SelectedShopWorkdayPeriodDetail)
                .Subscribe(shopWorkdayPeriodDetail =>
                {
                    Console.WriteLine($"ShopWorkdayPeriodDetail: {shopWorkdayPeriodDetail}");
                })
                .DisposeWith(disposable);

            // subscribe to the ShopWorkdayPeriodDetailEvent
            MessageBus.Current.Listen<ShopWorkdayPeriodDetailEvent>()
                .Subscribe(shopWorkdayPeriodDetailEvent =>
                {
                    // log the ShopWorkdayPeriodDetailMessage
                    Console.WriteLine(
                        $"{GetType().Name}: ShopWorkdayPeriodDetailMessage: {shopWorkdayPeriodDetailEvent.ShopWorkdayPeriodDetailMessage?.WorkdayPeriodDetailId}");

                    // set the ShopWorkdayPeriodDetail property
                    SelectedShopWorkdayPeriodDetail = shopWorkdayPeriodDetailEvent.ShopWorkdayPeriodDetailMessage;
                })
                .DisposeWith(disposable);
            
            // log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposable);
        });
    }
}