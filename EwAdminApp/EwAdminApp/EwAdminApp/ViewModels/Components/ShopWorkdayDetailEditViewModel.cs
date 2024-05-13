using System.Reactive;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Pos;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Text;
using System.Text.Json;
using EwAdminApp.Events;
using EwAdminApp.Models;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class ShopWorkdayDetailEditViewModel : ViewModelBase
{
    // this viewmodel is used to edit the details of a ShopWorkdayDetail
    // add a property named "SelectedShopWorkdayDetail" of type ShopWorkdayDetail
    // code here
    private ShopWorkdayDetail? _selectedShopWorkdayDetail;

    public ShopWorkdayDetail? SelectedShopWorkdayDetail
    {
        get => _selectedShopWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayDetail, value);
    }
    
    // add a property named "SelectedShopWorkdayDetailClone" of type ShopWorkdayDetail
    // code here
    private ShopWorkdayDetail? _selectedShopWorkdayDetailClone;
    
    public ShopWorkdayDetail? SelectedShopWorkdayDetailClone
    {
        get => _selectedShopWorkdayDetailClone;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayDetailClone, value);
    }

    // add a Reactive Command named "SaveCommand"
    // code here
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    
    // add a Reactive Command named "CancelCommand"
    // code here
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // add an IsBusy property of type bool
    // code here
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a constructor
    // code here
    public ShopWorkdayDetailEditViewModel()
    {
        // Create an observable that evaluates whether SaveCommand can execute
        // SaveCommand can be executed if SelectedShopWorkdayPeriodDetail is not null and IsBusy is false
        var canSave = this.WhenAnyValue(
            x => x.SelectedShopWorkdayDetail,
            x => x.IsBusy,
            (selectedShopWorkdayPeriodDetail, isBusy) => selectedShopWorkdayPeriodDetail != null && !isBusy);
        
        var canCancel = this.WhenAnyValue(
            x => x.IsBusy,
            isBusy => !isBusy);

        // Create the SaveCommand
        SaveCommand = ReactiveCommand.CreateFromTask(DoSave, canSave);
        
        // Create the CancelCommand
        CancelCommand = ReactiveCommand.CreateFromTask(DoCancel, canCancel);

        // when the SaveCommand is executing, set the IsBusy property to true
        SaveCommand.IsExecuting.Subscribe(isExecuting => IsBusy = isExecuting);

        // handle the exception when the SaveCommand is executed
        SaveCommand.ThrownExceptions.Subscribe(exception =>
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
        });

        // listen to the Message Bus for ShopWorkdayDetailEvent
        // when the event is received, set the SelectedShopWorkdayPeriodDetail property
        MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
            .Subscribe(shopWorkdayDetailEvent =>
            {
                SelectedShopWorkdayDetail = shopWorkdayDetailEvent.ShopWorkdayDetailMessage;
            });
    }

    private async Task DoSave()
    {
        try
        {
            // Save the SelectedShopWorkdayDetail
            // HTTP PATCH request to the API to update the ShopWorkdayDetail
            // API Endpoint: https://localhost:7045/api/PosAdmin/UpdateShopWorkdayDetail
            // Request Body: SelectedShopWorkdayDetail
            // Request headers: Authorization Bearer Token
            // code here
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            // check if SelectedShopWorkdayDetail is not null
            if (SelectedShopWorkdayDetail == null) return;

            var requestShopWorkdayDetail = new ShopWorkdayDetail
            {
                AccountId = SelectedShopWorkdayDetail.AccountId,
                ShopId = SelectedShopWorkdayDetail.ShopId,
                WorkdayDetailId = SelectedShopWorkdayDetail.WorkdayDetailId,
                WorkdayHeaderId = SelectedShopWorkdayDetail.WorkdayHeaderId,
                OpenDatetime = SelectedShopWorkdayDetail.OpenDatetime,
                CloseDatetime = SelectedShopWorkdayDetail.CloseDatetime,
                IsClosed = SelectedShopWorkdayDetail.IsClosed,
                Enabled = SelectedShopWorkdayDetail.Enabled
            };

            var request = new HttpRequestMessage(HttpMethod.Patch,
                "https://localhost:7045/api/PosAdmin/UpdateShopWorkdayDetail")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestShopWorkdayDetail), Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentLoginSettings.ApiKey);

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var resultShopWorkdayDetail = JsonSerializer.Deserialize<ShopWorkdayDetail>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            
            // update the SelectedShopWorkdayDetail with the resultShopWorkdayDetail using RxApp.MainThreadScheduler
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                SelectedShopWorkdayDetail = resultShopWorkdayDetail;
            });
            
            // send a ShopWorkdayDetailSavedEvent using the MessageBus
            MessageBus.Current.SendMessage(new ShopWorkdayDetailEvent(resultShopWorkdayDetail));
            
            // log the success message
            Console.WriteLine("ShopWorkdayDetail saved successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    // add a method named "DoCancel"
    // code here
    private Task DoCancel()
    {
        // reset the SelectedShopWorkdayDetail to the SelectedShopWorkdayDetailClone using RxApp.MainThreadScheduler
        // serialize the SelectedShopWorkdayDetailClone to JSON and deserialize it back to ShopWorkdayDetail
        // code here
        if (SelectedShopWorkdayDetailClone == null) return Task.CompletedTask;
        
        var selectedShopWorkdayDetailCloneJson = JsonSerializer.Serialize(SelectedShopWorkdayDetailClone);
        var selectedShopWorkdayDetailCloneObj = JsonSerializer.Deserialize<ShopWorkdayDetail>(selectedShopWorkdayDetailCloneJson);
        
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            SelectedShopWorkdayDetail = selectedShopWorkdayDetailCloneObj;
        });
        
        return Task.CompletedTask;
    }
}