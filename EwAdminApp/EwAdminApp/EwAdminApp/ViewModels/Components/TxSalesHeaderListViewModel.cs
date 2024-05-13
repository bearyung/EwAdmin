using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
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

    // Add a property for SearchText
    private string? _searchText;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
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

        // set the IsBusy property to true when the SearchCommand is executing
        SearchCommand.IsExecuting.Subscribe(isExecuting => { IsBusy = isExecuting; });

        // handle the exception when the SearchCommand is executed
        SearchCommand.ThrownExceptions.Subscribe(ex =>
        {
            Console.WriteLine("Failed to search for shop workday detail");
            Console.WriteLine(ex.Message);
        });

        // when SelectedShop or SelectedShopWorkdayDetail is changed, execute some code
        this.WhenAnyValue(
                x => x.SelectedShop,
                x => x.SelectedShopWorkdayDetail,
                (shop, shopWorkdayDetail) => new { shop, shopWorkdayDetail })
            .Subscribe(x =>
            {
                // clear the TxSalesHeaderList property
                RxApp.MainThreadScheduler.Schedule(() => TxSalesHeaderList?.Clear());

                // clear the selected TxSalesHeader
                SelectedTxSalesHeader = null;

                // clear the search text
                SearchText = null;
            });

        // ReactiveUI messagebus listen for the ShopEvent
        MessageBus.Current.Listen<ShopEvent>()
            .Subscribe(x =>
            {
                // console log the ShopEvent received from this component
                Console.WriteLine("TxSalesHeaderListViewModel: ShopEvent received: " + x.ShopMessage?.Name);

                // Set the SelectedShop property to the Shop property of the ShopEvent
                SelectedShop = x.ShopMessage;
            });

        // ReactiveUI messagebus listen for the ShopWorkdayDetailEvent
        MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
            .Subscribe(x =>
            {
                // console log the ShopWorkdayDetailEvent received from this component
                Console.WriteLine("TxSalesHeaderListViewModel: ShopWorkdayDetailEvent received: " +
                                  x.ShopWorkdayDetailMessage?.WorkdayDetailId);

                // Set the SelectedShopWorkdayDetail property to the ShopWorkdayDetail property of the ShopWorkdayDetailEvent
                SelectedShopWorkdayDetail = x.ShopWorkdayDetailMessage;
            });

        // ReactiveUI messagebus sendmessage if there any changes in the SelectedTxSalesHeader
        this.WhenAnyValue(x => x.SelectedTxSalesHeader)
            .Subscribe(x =>
            {
                // console log the SelectedTxSalesHeader
                Console.WriteLine("TxSalesHeaderListViewModel: SelectedTxSalesHeader changed: " + x?.TxSalesHeaderId);

                // send the SelectedTxSalesHeader to the TxPaymentListViewModel
                MessageBus.Current.SendMessage(new TxSalesHeaderMinEvent(SelectedTxSalesHeader));
            });
    }

    // Implement the DoSearch method
    // This method is called when the SearchCommand is executed
    // It sends a request to the server to get the TxSalesHeader list
    // The request is sent to the https://localhost:7045/api/PosAdmin/txSalesHeader?accountid=11377&shopid=5311&txDate=2023-08-14
    // The response is deserialized into a list of TxSalesHeader objects
    // The list is added to the TxSalesHeaderList property
    // code here
    private async Task DoSearch()
    {
        try
        {
            // Clear the TxSalesHeaderList property
            RxApp.MainThreadScheduler.Schedule(() => TxSalesHeaderList?.Clear());

            // Perform the search operation
            // Use the HttpClient registered in the DI container to call the API
            // Add the search results to the TxSalesHeaderList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://localhost:7045/api/PosAdmin/txSalesHeaderList?" +
                $"accountid={SelectedShop?.AccountId}" +
                $"&shopid={SelectedShop?.ShopId}" +
                $"&txDate={SelectedShopWorkdayDetail?.OpenDatetime:yyyy-MM-dd}");

            // add the header bearer token
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
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
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to search for TxSalesHeader");
            Console.WriteLine(ex.Message);
        }
    }
}