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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
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

    // add a cancellationTokenSource property for GetPaymentMethodList
    private CancellationTokenSource _getPaymentMethodListCancellationTokenSource = new();

    // add a cancellationTokenSource property for DoSave
    private CancellationTokenSource _doSaveCancellationTokenSource = new();

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

        // Create an observable that evaluates whether CancelCommand can execute
        // CancelCommand can be executed if IsBusy is false
        var canExecuteCancel = this.WhenAnyValue(
            x => x.IsBusy,
            isBusy => !isBusy);

        // implement the CancelCommand
        CancelCommand = ReactiveCommand.CreateFromTask(
            execute: DoCancel,
            canExecute: canExecuteCancel);

        // Init the PaymentMethodListCommand
        PaymentMethodListCommand = ReactiveCommand.CreateFromTask(
            execute: GetPaymentMethodList);

        this.WhenActivated((disposables) =>
        {
            // log when the ViewModel is activated
            Console.WriteLine($"{GetType().Name} activated");

            // set the IsBusy property to true when the SaveCommand is executing
            SaveCommand.IsExecuting.Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;

                    // set the IsBusy property
                    IsBusy = isExecuting;

                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus only if it is not the initial execution
                    if (!isInitial)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = isExecuting
                                    ? ActionStatus.StatusEnum.Executing
                                    : ActionStatus.StatusEnum.Completed,
                                Message = isExecuting ? "Saving TxPayment..." : "TxPayment saved"
                            }));
                    }
                })
                .DisposeWith(disposables);


            // Listen to the TxPayment Event, and set the SelectedTxPayment property and clone a copy of it for the Cancel command
            MessageBus.Current.Listen<TxPaymentEvent>()
                .Subscribe(txPaymentEvent =>
                {
                    // Clone a copy of the txPaymentEvent.TxPaymentMessage;
                    var serializedSelectedTxPayment = JsonSerializer.Serialize(txPaymentEvent.TxPaymentMessage);
                    SelectedTxPaymentClone = JsonSerializer.Deserialize<TxPayment>(serializedSelectedTxPayment);

                    // Set the SelectedTxPayment property using RxApp.MainThreadScheduler
                    RxApp.MainThreadScheduler.Schedule(() => { SelectedTxPayment = txPaymentEvent.TxPaymentMessage; });
                })
                .DisposeWith(disposables);

            // listen to the ShopEvent, and update the SelectedShop property
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(shopEvent => { SelectedShop = shopEvent.ShopMessage; })
                .DisposeWith(disposables);


            // handle the exception when the PaymentMethodListCommand is executed
            PaymentMethodListCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to get payment method list");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);

            // set the IsBusy property to true when the PaymentMethodListCommand is executing
            PaymentMethodListCommand.IsExecuting.Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;

                    // set the IsBusy property
                    IsBusy = isExecuting;

                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus only if it is not the initial execution
                    if (!isInitial)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = isExecuting
                                    ? ActionStatus.StatusEnum.Executing
                                    : ActionStatus.StatusEnum.Completed,
                                Message = isExecuting
                                    ? "Getting payment method list..."
                                    : "Payment method list retrieved"
                            }));
                    }
                })
                .DisposeWith(disposables);

            // Call the PaymentMethodListCommand when the SelectedShop property changes
            this.WhenAnyValue(x => x.SelectedShop)
                .Where(shop => shop != null)
                .Subscribe(_ =>
                {
                    PaymentMethodListCommand
                        .Execute()
                        .Subscribe()
                        .DisposeWith(disposables);
                })
                .DisposeWith(disposables);

            // set the SelectedPaymentMethod property when the SelectedTxPayment property changes
            this.WhenAnyValue(x => x.SelectedTxPayment)
                .Subscribe(txPayment =>
                {
                    if (SelectedTxPayment == null)
                    {
                        SelectedPaymentMethod = null;
                    }
                    else
                    {
                        SelectedPaymentMethod = AvailablePaymentMethodList?.FirstOrDefault(paymentMethod =>
                            paymentMethod.PaymentMethodId == txPayment?.PaymentMethodId);
                    }
                })
                .DisposeWith(disposables);

            // log when the ViewModel is deactivated
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");

                    // cancel the CancellationTokenSource
                    _getPaymentMethodListCancellationTokenSource.Cancel();
                    _doSaveCancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }

    private async Task GetPaymentMethodList()
    {
        try
        {
            // Cancel the previous search operation
            await _getPaymentMethodListCancellationTokenSource.CancelAsync();

            // Create a new CancellationTokenSource
            _getPaymentMethodListCancellationTokenSource = new CancellationTokenSource();
    
            // Get the cancellation token from the CancellationTokenSource
            var cancellationToken = _getPaymentMethodListCancellationTokenSource.Token;

            // Throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // Get the Header API Key from the LoginSettings from the Locator
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();

            // Get the HttpClient from the Locator
            var httpClient = Locator.Current.GetService<HttpClient>();

            // Check if the currentLoginSettings and httpClient are not null
            if (currentLoginSettings == null || httpClient == null) return;

            // Check if the SelectedShop is not null
            if (SelectedShop == null) return;

            // Create a request to get the payment method list
            // Request method: GET
            // Request header: Authorization with the token (Bearer token)
            // Request URL: /api/PosAdmin/paymentMethodList?accountid={accountId}&shopid={shopId}
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/paymentMethodList?" +
                $"accountid={SelectedShop.AccountId}&shopid={SelectedShop.ShopId}" +
                $"&page=1&pageSize=100");

            // Add the Authorization header to the request
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            // Send the request and get the response
            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Check if the response is successful
            if (!response.IsSuccessStatusCode) return;

            // Read the content of the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            // Deserialize the content to a list of PaymentMethod
            var resultPaymentMethodList = JsonSerializer.Deserialize<List<PaymentMethod>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

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
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(GetPaymentMethodList)} operation cancelled");
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
        if (SelectedTxPaymentClone == null) return Task.CompletedTask;

        var selectedTxPaymentCloneJson = JsonSerializer.Serialize(SelectedTxPaymentClone);
        var selectedTxPaymentCloneObj = JsonSerializer.Deserialize<TxPayment>(selectedTxPaymentCloneJson);

        RxApp.MainThreadScheduler.Schedule(() => { SelectedTxPayment = selectedTxPaymentCloneObj; });

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
            // Cancel the previous save operation
            await _doSaveCancellationTokenSource.CancelAsync();

            // Create a new CancellationTokenSource
            _doSaveCancellationTokenSource = new CancellationTokenSource();

            // Get the cancellation token from the CancellationTokenSource
            var cancellationToken = _doSaveCancellationTokenSource.Token;

            // Throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // Save the SelectedTxPayment by calling the API endpoint: /api/PosAdmin/updateTxPayment
            // Request method: PUT
            // Request header: Authorization with the token (Bearer token)
            // Request body: TxPayment with only the fields that need to be updated (accountid, shopid, txPaymentId, paymentMethodId, Enabled)
            // Response: TxPayment
            // code here

            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            // check if SelectedTxPayment is null or if any of the required fields are null
            if (SelectedTxPayment == null) return;

            var requestTxPayment = new TxPayment
            {
                AccountId = SelectedTxPayment!.AccountId,
                ShopId = SelectedTxPayment!.ShopId,
                TxPaymentId = SelectedTxPayment!.TxPaymentId,
                PaymentMethodId = SelectedPaymentMethod!.PaymentMethodId,
                Enabled = SelectedTxPayment!.Enabled
            };

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/PosAdmin/updateTxPayment")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestTxPayment), System.Text.Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var resultTxPayment = JsonSerializer.Deserialize<TxPayment>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var resultTxPaymentClone = JsonSerializer.Deserialize<TxPayment>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(DoSave)} operation cancelled");
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