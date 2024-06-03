using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class TxSalesHeaderDetailViewModel : ViewModelBase
{
    // properties
    // selectedTxSalesHeader
    private TxSalesHeaderMin? _selectedTxSalesHeaderMin;

    private TxSalesHeader? _selectedTxSalesHeader;

    private bool _isBusy;

    // add a command for DoSearch
    private ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    public TxSalesHeaderMin? SelectedTxSalesHeaderMin
    {
        get => _selectedTxSalesHeaderMin;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeaderMin, value);
    }

    public TxSalesHeader? SelectedTxSalesHeader
    {
        get => _selectedTxSalesHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeader, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a constructor
    // code here
    public TxSalesHeaderDetailViewModel()
    {
        // Create an observable that evaluates whether SearchCommand can execute
        // SearchCommand can be executed if SelectedTxSalesHeaderMin is not null and IsBusy is false
        var canExecuteSearch = this.WhenAnyValue(
            x => x.SelectedTxSalesHeaderMin,
            x => x.IsBusy,
            (selectedTxSalesHeaderMin, isBusy) => selectedTxSalesHeaderMin != null && !isBusy);

        // implement the SearchCommand
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);

        this.WhenActivated(disposables =>
        {
            // log the activation of the ViewModel
            Console.WriteLine($"{GetType().Name} activated");

            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for TxSalesHeader.");
                    Console.WriteLine(ex.Message);
                    
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // set the IsBusy property to true when the SearchCommand is executing
            // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus
            // amend the ExecutingCommandsCount property
            SearchCommand.IsExecuting
                .Subscribe(isExecuting =>
                {
                    // set the IsBusy property
                    IsBusy = isExecuting;
                    
                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus only if it is not the initial execution
                    if (isExecuting)
                    {
                        MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                            sourceTypeName: GetType().Name, 
                            isExecutionIncrement: true));
                        
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = ActionStatus.StatusEnum.Executing,
                                Message = "Searching for TxSalesHeader..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // use ReactiveUI MessageBus to subscribe the TxSalesHeaderMinEvent
            MessageBus.Current.Listen<TxSalesHeaderMinEvent>()
                .Subscribe(txSalesHeaderMinEvent =>
                {
                    // console log the event
                    Console.WriteLine(
                        $"Received {txSalesHeaderMinEvent.GetType().Name}: {txSalesHeaderMinEvent.TxSalesHeaderMinMessage?.TxSalesHeaderId}");

                    // terminate the previous SearchCommand
                    SearchCommand.Dispose();

                    // clear the SelectedTxSalesHeader property
                    RxApp.MainThreadScheduler.Schedule(() => SelectedTxSalesHeader = null);

                    // set the SelectedTxSalesHeaderMin property
                    SelectedTxSalesHeaderMin = txSalesHeaderMinEvent.TxSalesHeaderMinMessage;
                })
                .DisposeWith(disposables);
            
            // use ReactiveUI MessageBus to subscribe the TxSalesHeaderEvent
            MessageBus.Current.Listen<TxSalesHeaderEvent>()
                .Subscribe(txSalesHeaderEvent =>
                {
                    // console log the event
                    Console.WriteLine(
                        $"Received {txSalesHeaderEvent.GetType().Name}: {txSalesHeaderEvent.TxSalesHeaderMessage?.TxSalesHeaderId}");

                    // set the SelectedTxSalesHeader property
                    RxApp.MainThreadScheduler.Schedule(() => SelectedTxSalesHeader = txSalesHeaderEvent.TxSalesHeaderMessage);
                })
                .DisposeWith(disposables);
            
            // when the SelectedTxSalesHeaderMin property changes, clear the SelectedTxSalesHeader property
            // execute the SearchCommand
            this.WhenAnyValue(x => x.SelectedTxSalesHeaderMin)
                .Subscribe(txSalesHeaderMin =>
                {
                    SelectedTxSalesHeader = null;
                    if (txSalesHeaderMin != null)
                    {
                        SearchCommand.Execute()
                            .Subscribe()
                            .DisposeWith(disposables);
                    }
                })
                .DisposeWith(disposables);
            
            // use ReactiveUI MessageBus to send the TxSalesHeaderEvent when there's changes in the SelectedTxSalesHeader property
            this.WhenAnyValue(x => x.SelectedTxSalesHeader)
                .Subscribe(selectedTxSalesHeader =>
                {
                    MessageBus.Current.SendMessage(new TxSalesHeaderEvent(selectedTxSalesHeader));
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
                            Message = "TxSalesHeader search completed."
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

    // method: DoSearch 
    // get the accountId, shopId and txSalesHeaderId from the selectedTxSalesHeaderMin
    // and use the API endpoint to get the TxSalesHeader object
    // use the HTTP GET method

    // API endpoint: /api/PosAdmin/txSalesHeader?accountId={accountId}&shopId={shopId}&txSalesHeaderId={txSalesHeaderId}
    // code here
    public async Task DoSearch()
    {
        try
        {
            // Cancel the previous search operation
            await _cancellationTokenSource.CancelAsync();

            // Create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // Get the CancellationToken from the CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token;

            // Throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // perform the search for the detail of TxPayment
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/txSalesHeader?accountId={SelectedTxSalesHeaderMin?.AccountId}&shopId={SelectedTxSalesHeaderMin?.ShopId}&txSalesHeaderId={SelectedTxSalesHeaderMin?.TxSalesHeaderId}");

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
            var resultTxSalesHeader = JsonSerializer.Deserialize<TxSalesHeader>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // set the result to the SelectedTxSalesHeader property
            SelectedTxSalesHeader = resultTxSalesHeader;
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