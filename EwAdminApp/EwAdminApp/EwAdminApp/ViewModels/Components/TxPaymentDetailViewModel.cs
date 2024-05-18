using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class TxPaymentDetailViewModel : ViewModelBase
{
    // Add a property for SelectedTxPayment
    private TxPayment? _selectedTxPayment;

    public TxPayment? SelectedTxPayment
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

    // Add a property for SelectedTxPaymentMin
    private TxPaymentMin? _selectedTxPaymentMin;

    public TxPaymentMin? SelectedTxPaymentMin
    {
        get => _selectedTxPaymentMin;
        set => this.RaiseAndSetIfChanged(ref _selectedTxPaymentMin, value);
    }

    // add a command for DoSearch
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a constructor
    public TxPaymentDetailViewModel()
    {
        // Create an observable that evaluates whether SearchCommand can execute
        // SearchCommand can be executed if SelectedTxPaymentMin is not null and IsBusy is false
        var canExecuteSearch = this.WhenAnyValue(
            x => x.SelectedTxPaymentMin,
            x => x.IsBusy,
            (selectedTxPaymentMin, isBusy) => selectedTxPaymentMin != null && !isBusy);

        // implement the SearchCommand
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);

        this.WhenActivated((disposables) =>
        {
            // log the activation of the ViewModel
            Console.WriteLine($"{GetType().Name} activated");
            
            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for txpayment");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);

            // set the IsBusy property to true when the SearchCommand is executing
            SearchCommand.IsExecuting.Subscribe(isExecuting =>
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
                                Message = isExecuting ? "Searching TxPayment..." : "TxPayment search completed"
                            }));
                    }
                })
                .DisposeWith(disposables);

            // use ReactiveUI MessageBus to subscribe to the TxPaymentMinEvent
            MessageBus.Current.Listen<TxPaymentMinEvent>()
                .Subscribe(txPaymentMinEvent =>
                {
                    // console log the TxPaymentMinEvent
                    Console.WriteLine(
                        $"{GetType().Name}: TxPaymentMinEvent received: {txPaymentMinEvent.TxPaymentMinMessage?.TxPaymentId}");

                    // terminate previous SearchCommand
                    SearchCommand.Dispose();

                    // clear the SelectedTxPayment property
                    SelectedTxPayment = null;

                    // set the SelectedTxPaymentMin property to the TxPaymentMin from the event
                    SelectedTxPaymentMin = txPaymentMinEvent.TxPaymentMinMessage;

                    // execute the SearchCommand with a throttle of 300 milliseconds
                    SearchCommand
                        .Execute()
                        .Throttle(TimeSpan.FromMilliseconds(300))
                        .Subscribe();
                })
                .DisposeWith(disposables);

            // use the MessageBus to subscribe to the TxPaymentEvent
            MessageBus.Current.Listen<TxPaymentEvent>()
                .Subscribe(txPaymentEvent =>
                {
                    // console log the TxPaymentEvent
                    Console.WriteLine(
                        $"{GetType().Name}: TxPaymentEvent received: {txPaymentEvent.TxPaymentMessage?.TxPaymentId}");

                    // set the SelectedTxPayment property to the TxPayment from the event
                    SelectedTxPayment = txPaymentEvent.TxPaymentMessage;
                })
                .DisposeWith(disposables);

            // when the SelectedTxPayment property changes, use ReactiveUI MessageBus to publish the TxPaymentEvent
            this.WhenAnyValue(x => x.SelectedTxPayment)
                .Subscribe(txPayment => { MessageBus.Current.SendMessage(new TxPaymentEvent(txPayment)); })
                .DisposeWith(disposables);
            
            // log the deactivation of the ViewModel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    // add an async method DoSearch
    // this method will be called when the SearchCommand is executed
    // this method will be used to search the detail of TxPayment
    // based on the selected TxPaymentMin
    // the result will be displayed in the view
    // API call will be used to get the detail of TxPayment
    // API Endpoint: /api/PosAdmin/txPayment?accountid={accountId}&shopid={shopId}&txPaymentId={txPaymentId}
    // code here
    public async Task DoSearch()
    {
        try
        {
            // perform the search for the detail of TxPayment
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/txPayment?accountid={SelectedTxPaymentMin?.AccountId}&shopid={SelectedTxPaymentMin?.ShopId}&txPaymentId={SelectedTxPaymentMin?.TxPaymentId}");

            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var resultTxPayment = JsonSerializer.Deserialize<TxPayment>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // set the result to the SelectedTxPayment property
            SelectedTxPayment = resultTxPayment;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}