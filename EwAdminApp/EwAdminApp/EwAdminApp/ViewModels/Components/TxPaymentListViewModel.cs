using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class TxPaymentListViewModel : ViewModelBase
{
    // All imlpelementations are use ReactiveUI
    // Add a property for ObservableCollection TxPayment List
    private ObservableCollection<TxPaymentMin>? _txPaymentList;

    public ObservableCollection<TxPaymentMin>? TxPaymentList
    {
        get => _txPaymentList;
        set => this.RaiseAndSetIfChanged(ref _txPaymentList, value);
    }

    // Add a property for SelectedTxPayment
    private TxPaymentMin? _selectedTxPayment;

    public TxPaymentMin? SelectedTxPayment
    {
        get => _selectedTxPayment;
        set => this.RaiseAndSetIfChanged(ref _selectedTxPayment, value);
    }

    // Add a property for IsBusy
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // Add a command for DoSearch
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a property for SelectedTxSalesHeader
    private TxSalesHeaderMin? _selectedTxSalesHeader;

    public TxSalesHeaderMin? SelectedTxSalesHeader
    {
        get => _selectedTxSalesHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeader, value);
    }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // Constructor
    public TxPaymentListViewModel()
    {
        // init the TxPaymentList
        TxPaymentList = new ObservableCollection<TxPaymentMin>();

        // Create an observable that evaluates whether SearchCommand can execute
        // SearchCommand can be executed if SelectedTxSalesHeader is not null and IsBusy is false
        var canExecuteSearch = this.WhenAnyValue(
            x => x.SelectedTxSalesHeader,
            x => x.IsBusy,
            (selectedTxSalesHeader, isBusy) => selectedTxSalesHeader != null && !isBusy);

        // implement the SearchCommand
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);

        this.WhenActivated((disposables) =>
        {
            // log the activation of the ViewModel
            Console.WriteLine($"{GetType().Name} activated");

            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for txPayment");
                    Console.WriteLine(ex.Message);
                    
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // set the IsBusy property to true when the SearchCommand is executing
            SearchCommand.IsExecuting
                .Subscribe(isExecuting =>
                {
                    // set the IsBusy property
                    IsBusy = isExecuting;

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus
                    if (isExecuting)
                    {
                        MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                            sourceTypeName: GetType().Name, 
                            isExecutionIncrement: true));
                        
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = ActionStatus.StatusEnum.Executing,
                                Message = "Searching for TxPayment list..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // use ReactiveUI MessageBus to subscribe to the TxSalesHeaderMinEvent
            MessageBus.Current.Listen<TxSalesHeaderMinEvent>()
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Subscribe(txSalesHeaderMinEvent =>
                {
                    // console log the TxSalesHeaderMinEvent
                    Console.WriteLine(
                        $"{GetType().Name}: TxSalesHeaderMinEvent received: {txSalesHeaderMinEvent.TxSalesHeaderMinMessage?.TxSalesHeaderId}");

                    // clear the TxPaymentList property
                    RxApp.MainThreadScheduler.Schedule(() => TxPaymentList?.Clear());

                    // set the SelectedTxSalesHeader property to the TxSalesHeaderMin property of the TxSalesHeaderMinEvent
                    SelectedTxSalesHeader = txSalesHeaderMinEvent.TxSalesHeaderMinMessage;

                    // execute the SearchCommand
                    if (SelectedTxSalesHeader != null)
                    {
                        SearchCommand.Execute()
                            .Subscribe()
                            .DisposeWith(disposables);
                    }
                })
                .DisposeWith(disposables);

            // use ReactiveUI MessageBus to sendmessage when there's changes in the SelectedTxPayment property
            this.WhenAnyValue(x => x.SelectedTxPayment)
                .Subscribe(selectedTxPayment =>
                {
                    // emit the TxPaymentMinEvent using the ReactiveUI MessageBus
                    MessageBus.Current.SendMessage(new TxPaymentMinEvent(selectedTxPayment));
                })
                .DisposeWith(disposables);
            
            // Subscribe to the SearchCommand's Executed observable
            // Subscribe to the SearchCommand itself
            SearchCommand.Subscribe(_ =>
                {
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                    
                    MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                        new ActionStatus
                        {
                            ActionStatusEnum = ActionStatus.StatusEnum.Completed,
                            Message = "TxPayment list search completed."
                        }));
                })
                .DisposeWith(disposables);

            // log the deactivation of the ViewModel
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");

                    // cancel the CancellationTokenSource
                    _cancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }

    // implement the async method DoSearch
    // this method will be called when the SearchCommand is executed
    // this method will search for the txpayment based on the selected txsalesheader
    // the result will be stored in the TxPaymentList property
    // API endpoint to use: GET /api/PosAdmin/txPaymentList?accountid={accountId}&shopid={shopId}&txSalesHeaderId={txSalesHeaderId}
    private async Task DoSearch()
    {
        try
        {
            // cancel the previous search operation
            await _cancellationTokenSource.CancelAsync();

            // create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // get the CancellationToken from the CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token;

            // throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // clear the TxPaymentList property
            RxApp.MainThreadScheduler.Schedule(() => TxPaymentList?.Clear());

            // perform the search for txpayment
            // code here
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request =
                new HttpRequestMessage(HttpMethod.Get,
                    $"/api/PosAdmin/txPaymentList?accountid={SelectedTxSalesHeader?.AccountId}&shopid={SelectedTxSalesHeader?.ShopId}&txSalesHeaderId={SelectedTxSalesHeader?.TxSalesHeaderId}");

            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // log the error
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Error: {errorContent}");
                
                // throw an exception with error code and content
                throw new Exception($"Error: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var resultTxPaymentList = JsonSerializer.Deserialize<List<TxPaymentMin>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // add the search results to the TxPaymentList property
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                TxPaymentList?.Clear();

                foreach (var txPayment in resultTxPaymentList)
                {
                    TxPaymentList?.Add(txPayment);
                }

                // add the first TxPayment in the list to the SelectedTxPayment property
                SelectedTxPayment = TxPaymentList?.FirstOrDefault();
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{nameof(DoSearch)} operation cancelled");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}