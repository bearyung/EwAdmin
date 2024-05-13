using System.Reactive;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EwAdminApp.Models;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class TxPaymentDetailEditViewModel : ViewModelBase
{
    // Add a property for SelectedShop
    private Shop? _selectedShop;
    
    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }
    
    // Add a property for AvailablePaymentMethodList
    private ObservableCollection<PaymentMethod>? _availablePaymentMethodList;
    
    public ObservableCollection<PaymentMethod>? AvailablePaymentMethodList
    {
        get => _availablePaymentMethodList;
        set => this.RaiseAndSetIfChanged(ref _availablePaymentMethodList, value);
    }
    
    // Add a property for SelectedPaymentMethod
    private PaymentMethod? _selectedPaymentMethod;
    
    public PaymentMethod? SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => this.RaiseAndSetIfChanged(ref _selectedPaymentMethod, value);
    }
    
    // Add a property for SelectedTxPayment
    private TxPayment? _selectedTxPayment;
    
    public TxPayment? SelectedTxPayment
    {
        get => _selectedTxPayment;
        set => this.RaiseAndSetIfChanged(ref _selectedTxPayment, value);
    }
    
    // Add a property for SelectedTxPaymentClone
    private TxPayment? _selectedTxPaymentClone;
    
    public TxPayment? SelectedTxPaymentClone
    {
        get => _selectedTxPaymentClone;
        set => this.RaiseAndSetIfChanged(ref _selectedTxPaymentClone, value);
    }
    
    // Add a property for IsBusy
    private bool _isBusy;
    
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    // Add a Command for Save
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    
    // Add a Command for Cancel
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    
    // Add a Command for GetPaymentMethodList
    public ReactiveCommand<Unit, Unit> PaymentMethodListCommand { get; }
    
    // Add a constructor
    public TxPaymentDetailEditViewModel()
    {
        // init the AvailablePaymentMethodList property
        AvailablePaymentMethodList = new ObservableCollection<PaymentMethod>();
        
        // Create an observable that evaluates whether SaveCommand can execute
        // SaveCommand can be executed if SelectedTxPayment is not null and IsBusy is false
        var canExecuteSave = this.WhenAnyValue(
            x => x.SelectedTxPayment,
            x => x.IsBusy,
            (selectedTxPayment, isBusy) => selectedTxPayment != null && !isBusy);
        
        // implement the SaveCommand
        SaveCommand = ReactiveCommand.CreateFromTask(
            execute: DoSave,
            canExecute: canExecuteSave);
        
        // set the IsBusy property to true when the SaveCommand is executing
        SaveCommand.IsExecuting.Subscribe(isExecuting => IsBusy = isExecuting);
        
        // Create an observable that evaluates whether CancelCommand can execute
        // CancelCommand can be executed if IsBusy is false
        var canExecuteCancel = this.WhenAnyValue(
            x => x.IsBusy,
            isBusy => !isBusy);
        
        // implement the CancelCommand
        CancelCommand = ReactiveCommand.CreateFromTask(
            execute: DoCancel,
            canExecute: canExecuteCancel);
        
        // Listen to the TxPayment Event, and set the SelectedTxPayment property and clone a copy of it for the Cancel command
        MessageBus.Current.Listen<TxPaymentEvent>()
            .Subscribe(txPaymentEvent =>
            {
                // Clone a copy of the txPaymentEvent.TxPaymentMessage;
                var serializedSelectedTxPayment = JsonSerializer.Serialize(txPaymentEvent.TxPaymentMessage);
                SelectedTxPaymentClone = JsonSerializer.Deserialize<TxPayment>(serializedSelectedTxPayment);
                
                // Set the SelectedTxPayment property using RxApp.MainThreadScheduler
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    SelectedTxPayment = txPaymentEvent.TxPaymentMessage;
                });
            });
        
        // listen to the ShopEvent, and update the SelectedShop property
        MessageBus.Current.Listen<ShopEvent>()
            .Subscribe(shopEvent =>
            {
                SelectedShop = shopEvent.ShopMessage;
            });
        
        // Init the PaymentMethodListCommand
        PaymentMethodListCommand = ReactiveCommand.CreateFromTask(
            execute: GetPaymentMethodList);
        
        // handle the exception when the PaymentMethodListCommand is executed
        PaymentMethodListCommand.ThrownExceptions.Subscribe(ex =>
        {
            Console.WriteLine("Failed to get payment method list");
            Console.WriteLine(ex.Message);
        });
        
        // set the IsBusy property to true when the PaymentMethodListCommand is executing
        PaymentMethodListCommand.IsExecuting.Subscribe(isExecuting => IsBusy = isExecuting);
        
        // Call the PaymentMethodListCommand when the SelectedShop property changes
        this.WhenAnyValue(x => x.SelectedShop)
            .Where(shop => shop != null)
            .Subscribe(shop =>
            {
                PaymentMethodListCommand.Execute().Subscribe();
            });
        
        // set the SelectedPaymentMethod property when the SelectedTxPayment property changes
        this.WhenAnyValue(x => x.SelectedTxPayment)
            .Where(txPayment => txPayment != null)
            .Subscribe(txPayment =>
            {
                SelectedPaymentMethod = AvailablePaymentMethodList?.FirstOrDefault(paymentMethod =>
                    paymentMethod.PaymentMethodId == txPayment?.PaymentMethodId);
            });
    }

    private async Task GetPaymentMethodList()
    {
        try
        {
            // Get the Header API Key from the LoginSettings from the Locator
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            
            // Get the HttpClient from the Locator
            var httpClient = Locator.Current.GetService<HttpClient>();
            
            // Check if the currentLoginSettings and httpClient are not null
            if(currentLoginSettings == null || httpClient == null) return;
            
            // Check if the SelectedShop is not null
            if(SelectedShop == null) return;
            
            // Create a request to get the payment method list
            // Request method: GET
            // Request header: Authorization with the token (Bearer token)
            // Request URL: https://localhost:7045/api/PosAdmin/paymentMethodList?accountid={accountId}&shopid={shopId}
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://localhost:7045/api/PosAdmin/paymentMethodList?accountid={SelectedShop.AccountId}&shopid={SelectedShop.ShopId}");
            
            // Add the Authorization header to the request
            request.Headers.Add("Authorization" , $"Bearer {currentLoginSettings.ApiKey}");
            
            // Send the request and get the response
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            
            // Check if the response is successful
            if(!response.IsSuccessStatusCode) return;
            
            // Read the content of the response
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // Deserialize the content to a list of PaymentMethod
            var resultPaymentMethodList = JsonSerializer.Deserialize<List<PaymentMethod>>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true})??[];
            
            // Update the AvailablePaymentMethodList property using RxApp.MainThreadScheduler
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                AvailablePaymentMethodList?.Clear();
                foreach (var paymentMethod in resultPaymentMethodList)
                {
                    AvailablePaymentMethodList?.Add(paymentMethod);
                }
            });
        }
        catch (Exception e)
        {
            // Log the exception message
            Console.WriteLine(e);
            throw;
        }
    }

    // implement the DoCancel method
    // This method will be called when the CancelCommand is executed
    // It will reset the SelectedTxPayment property
    private Task DoCancel()
    {
        // reset the SelectedTxPayment to the SelectedTxPaymentClone using RxApp.MainThreadScheduler
        // serialize the SelectedTxPaymentClone to JSON and deserialize it back to SelectedTxPayment
        // code here
        if(SelectedTxPaymentClone == null) return Task.CompletedTask;
        
        var selectedTxPaymentCloneJson = JsonSerializer.Serialize(SelectedTxPaymentClone);
        var selectedTxPaymentCloneObj = JsonSerializer.Deserialize<TxPayment>(selectedTxPaymentCloneJson);
        
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            SelectedTxPayment = selectedTxPaymentCloneObj;
        });
        
        return Task.CompletedTask;
    }
    
    // implement the DoSave method
    // This method will be called when the SaveCommand is executed
    // It will save the SelectedTxPayment
    private async Task DoSave()
    {
        // Save the SelectedTxPayment
        // Add a try-catch block to handle exceptions
        try
        {
            // Save the SelectedTxPayment by calling the API endpoint
            // API endpoint: https://localhost:7045/api/PosAdmin/updateTxPayment
            // Request method: PUT
            // Request header: Authorization with the token (Bearer token)
            // Request body: TxPayment with only the fields that need to be updated (accountid, shopid, txPaymentId, paymentMethodId, Enabled)
            // Response: TxPayment
            // code here
            
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if(currentLoginSettings == null) return;
            
            var httpClient = Locator.Current.GetService<HttpClient>();
            if(httpClient == null) return;
            
            // check if SelectedTxPayment is null or if any of the required fields are null
            if(SelectedTxPayment == null) return;
            
            var requestTxPayment = new TxPayment
            {
                AccountId = SelectedTxPayment!.AccountId,
                ShopId = SelectedTxPayment!.ShopId,
                TxPaymentId = SelectedTxPayment!.TxPaymentId,
                PaymentMethodId = SelectedPaymentMethod!.PaymentMethodId,
                Enabled = SelectedTxPayment!.Enabled
            };
            
            var request = new HttpRequestMessage(HttpMethod.Patch, "https://localhost:7045/api/PosAdmin/updateTxPayment")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestTxPayment), System.Text.Encoding.UTF8, "application/json")
            };
            
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");
            
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            
            if(!response.IsSuccessStatusCode) return;
            
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            var resultTxPayment = JsonSerializer.Deserialize<TxPayment>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            
            var resultTxPaymentClone = JsonSerializer.Deserialize<TxPayment>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            
            // update the SelectedTxPayment property using RxApp.MainThreadScheduler
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                SelectedTxPayment = resultTxPayment;
                SelectedTxPaymentClone = resultTxPaymentClone;
            });
            
            // use the MessageBus to publish the TxPaymentEvent
            MessageBus.Current.SendMessage(new TxPaymentEvent(resultTxPayment));
            
            // Log the success message
            Console.WriteLine("TxPayment saved successfully");
        }
        catch (Exception ex)
        {
            // Log the exception message
            Console.WriteLine("Failed to save TxPayment");
            Console.WriteLine(ex.Message);

            throw;
        }
    }

}