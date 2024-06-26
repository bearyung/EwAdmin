using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

public class TxSalesHeaderListViewModel : ViewModelBase
{
    // All imlpelementations are use ReactiveUI
    // Add a property for ObservableCollection TxSalesHeader List
    private ObservableCollection<TxSalesHeaderMin>? _txSalesHeaderList;

    public ObservableCollection<TxSalesHeaderMin>? TxSalesHeaderList
    {
        get => _txSalesHeaderList;
        set => this.RaiseAndSetIfChanged(ref _txSalesHeaderList, value);
    }

    // Add a property for SelectedTxSalesHeader
    private TxSalesHeaderMin? _selectedTxSalesHeader;

    public TxSalesHeaderMin? SelectedTxSalesHeader
    {
        get => _selectedTxSalesHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeader, value);
    }

    // Add a property for IsBusy
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // Add properties for SearchText
    private string? _searchTextTxSalesHeaderId;

    public string? SearchTextTxSalesHeaderId
    {
        get => _searchTextTxSalesHeaderId;
        set => this.RaiseAndSetIfChanged(ref _searchTextTxSalesHeaderId, value);
    }

    private string? _searchTextCusCount;
    
    public string? SearchTextCusCount
    {
        get => _searchTextCusCount;
        set => this.RaiseAndSetIfChanged(ref _searchTextCusCount, value);
    }
    
    private string? _searchTextTableCode;
    
    public string? SearchTextTableCode
    {
        get => _searchTextTableCode;
        set => this.RaiseAndSetIfChanged(ref _searchTextTableCode, value);
    }
    
    private string? _searchTextAmountTotal;
    
    public string? SearchTextAmountTotal
    {
        get => _searchTextAmountTotal;
        set => this.RaiseAndSetIfChanged(ref _searchTextAmountTotal, value);
    }

    // add a property for SelectedShop
    private Shop? _selectedShop;

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    // add a property for SelectedShopWorkdayDetail
    private ShopWorkdayDetail? _selectedShopWorkdayDetail;

    public ShopWorkdayDetail? SelectedShopWorkdayDetail
    {
        get => _selectedShopWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayDetail, value);
    }

    // Add a property for SearchCommand
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    
    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // add the constructor
    public TxSalesHeaderListViewModel()
    {
        // Initialize the TxSalesHeaderList property
        TxSalesHeaderList = new ObservableCollection<TxSalesHeaderMin>();

        // Create an observable that evaluates whether SearchCommand can execute
        // canExecuteSearch is true only when SelectedShop is not null and SelectedShopWorkdayDetail is not null
        var canExecuteSearch = this.WhenAnyValue(
            x => x.SelectedShop,
            x => x.SelectedShopWorkdayDetail,
            (shop, shopWorkdayDetail) => shop != null && shopWorkdayDetail != null
        );

        // Initialize the SearchCommand property
        // Bind the SearchCommand to the canExecuteSearch observable
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);

        this.WhenActivated((disposables) =>
        {
            // log the activation of the ViewModel
            Console.WriteLine($"{GetType().Name} activated");
            
            // set the IsBusy property to true when the SearchCommand is executing
            SearchCommand.IsExecuting.Subscribe(isExecuting =>
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
                                Message = "Searching for TxSalesHeader list..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for shop workday detail");
                    Console.WriteLine(ex.Message);
                    
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // when SelectedShop or SelectedShopWorkdayDetail is changed, execute some code
            this.WhenAnyValue(
                    x => x.SelectedShop,
                    x => x.SelectedShopWorkdayDetail,
                    (shop, shopWorkdayDetail) => new { shop, shopWorkdayDetail })
                .Subscribe(_ =>
                {
                    // clear the TxSalesHeaderList property
                    RxApp.MainThreadScheduler.Schedule(() => TxSalesHeaderList?.Clear());

                    // clear the selected TxSalesHeader
                    SelectedTxSalesHeader = null;

                    // clear the search text
                    SearchTextTxSalesHeaderId = null;
                })
                .DisposeWith(disposables);

            // ReactiveUI messagebus listen for the ShopEvent
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(x =>
                {
                    // console log the ShopEvent received from this component
                    Console.WriteLine($"{GetType().Name}: ShopEvent received: " + x.ShopMessage?.Name);

                    // Set the SelectedShop property to the Shop property of the ShopEvent
                    SelectedShop = x.ShopMessage;
                })
                .DisposeWith(disposables);

            // ReactiveUI messagebus listen for the ShopWorkdayDetailEvent
            MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
                .Subscribe(x =>
                {
                    // console log the ShopWorkdayDetailEvent received from this component
                    Console.WriteLine($"{GetType().Name}: ShopWorkdayDetailEvent received: " +
                                      x.ShopWorkdayDetailMessage?.WorkdayDetailId);

                    // Set the SelectedShopWorkdayDetail property to the ShopWorkdayDetail property of the ShopWorkdayDetailEvent
                    SelectedShopWorkdayDetail = x.ShopWorkdayDetailMessage;
                })
                .DisposeWith(disposables);

            // ReactiveUI messagebus sendmessage if there is any changes in the SelectedTxSalesHeader
            this.WhenAnyValue(x => x.SelectedTxSalesHeader)
                .Subscribe(x =>
                {
                    // console log the SelectedTxSalesHeader
                    Console.WriteLine(
                        $"{GetType().Name}: SelectedTxSalesHeader changed: " + x?.TxSalesHeaderId);

                    // send the SelectedTxSalesHeader to the TxPaymentListViewModel
                    MessageBus.Current.SendMessage(new TxSalesHeaderMinEvent(SelectedTxSalesHeader));
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
                            Message = "TxSalesHeader search completed"
                        }));
                })
                .DisposeWith(disposables);
            
            // console log when the ViewModel is deactivated
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");
                    
                    // cancel the CancellationTokenSource
                    _cancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }

    // Implement the DoSearch method
    // This method is called when the SearchCommand is executed
    // It sends a request to the server to get the TxSalesHeader list
    // The request is sent to the /api/PosAdmin/txSalesHeader?accountid=11377&shopid=5311&txDate=2023-08-14
    // The response is deserialized into a list of TxSalesHeader objects
    // The list is added to the TxSalesHeaderList property
    // code here
    private async Task DoSearch()
    {
        try
        {
            // Cancel the previous search operation
            await _cancellationTokenSource.CancelAsync();
            
            // Create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            
            // get the CancellationToken from the CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token;
            
            // throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();
            
            // Clear the TxSalesHeaderList property
            RxApp.MainThreadScheduler.Schedule(() => TxSalesHeaderList?.Clear());

            // Perform the search operation
            // Use the HttpClient registered in the DI container to call the API
            // Add the search results to the TxSalesHeaderList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;
            
            // create the parameters of amountTotalGte, amountTotalLte from SearchTextAmountTotal for the request
            // input format of SearchTextAmountTotal is "1000:2000" or "1000" or "1000:" or ":2000"
            var amountTotalGte = "";
            var amountTotalLte = "";
            if (!string.IsNullOrEmpty(SearchTextAmountTotal))
            {
                var amountTotaArrl = SearchTextAmountTotal.Split(':');
                if (amountTotaArrl.Length == 1)
                {
                    // case of ":1000", user want amount less than or equal to 1000
                    if (amountTotaArrl[0].StartsWith(":"))
                    {
                        amountTotalLte = amountTotaArrl[0].Substring(1);
                    }
                    // case of "1000:", user want amount greater than or equal to 1000
                    else if (amountTotaArrl[0].EndsWith(":"))
                    {
                        amountTotalGte = amountTotaArrl[0];
                    }
                    // case of "1000", user want exact amount
                    else
                    {
                        amountTotalLte = amountTotaArrl[0];
                        amountTotalGte = amountTotaArrl[0];
                    }
                }
                // case of "1000:2000", user want amount between 1000 and 2000 (inclusive)
                else if (amountTotaArrl.Length == 2)
                {
                    amountTotalGte = amountTotaArrl[0];
                    amountTotalLte = amountTotaArrl[1];
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/txSalesHeaderList?" +
                $"accountid={SelectedShop?.AccountId}" +
                $"&shopid={SelectedShop?.ShopId}" +
                $"&txDateGte={SelectedShopWorkdayDetail?.OpenDatetime:yyyy-MM-dd}" +
                $"&txDateLte={SelectedShopWorkdayDetail?.OpenDatetime:yyyy-MM-dd}" +
                $"&txSalesHeaderId={SearchTextTxSalesHeaderId}" + 
                $"&cusCountGte={SearchTextCusCount}" +
                $"&tableCode={SearchTextTableCode}" +
                $"&amountTotalGte={amountTotalGte}" +
                $"&amountTotalLte={amountTotalLte}");

            // add the header bearer token
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // log the error
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Error: {errorContent}");
                
                // throw an exception with error code and content
                throw new Exception($"Error: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var resultTxSalesHeaderList = JsonSerializer.Deserialize<List<TxSalesHeaderMin>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // Add the search results to the TxSalesHeaderList property
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                foreach (var txSalesHeader in resultTxSalesHeaderList)
                {
                    TxSalesHeaderList?.Add(txSalesHeader);
                }

                // add the first TxSalesHeader in the list to the SelectedTxSalesHeader property
                SelectedTxSalesHeader = TxSalesHeaderList?.FirstOrDefault();
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{nameof(DoSearch)} operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to search for TxSalesHeader");
            Console.WriteLine(ex.Message);
        }
    }
}