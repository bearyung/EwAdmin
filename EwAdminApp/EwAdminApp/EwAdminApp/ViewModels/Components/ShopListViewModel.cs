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

public class ShopListViewModel : ViewModelBase
{
    private ObservableCollection<Shop> _shopList = [];
    private Shop? _selectedShop;
    private bool _isBusy;
    private string? _searchTextAccountId;
    private string? _searchTextShopId;

    public ObservableCollection<Shop> ShopList
    {
        get => _shopList;
        set => this.RaiseAndSetIfChanged(ref _shopList, value);
    }

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string? SearchTextAccountId
    {
        get => _searchTextAccountId;
        set => this.RaiseAndSetIfChanged(ref _searchTextAccountId, value);
    }
    
    public string? SearchTextShopId
    {
        get => _searchTextShopId;
        set => this.RaiseAndSetIfChanged(ref _searchTextShopId, value);
    }

    public ReactiveCommand<Unit, Unit>? SearchCommand { get; }

    public ShopListViewModel()
    {
        // Initialize the ShopList property
        ShopList = new ObservableCollection<Shop>();

        // Initialize the SearchCommand property
        SearchCommand = ReactiveCommand.CreateFromTask(DoSearch);

        this.WhenAnyValue(x => x.SelectedShop)
            .Subscribe(shop =>
            {
                Console.WriteLine("Checkpoint 1: SelectedShop changed: " + shop?.Name);

                // Any other logic needed when a shop is selected
                // emit the ShopEvent using the ReactiveUI MessageBus
                MessageBus.Current.SendMessage(new ShopEvent(shop));
            });

        // set the IsBusy property to True when the SearchCommand is executing, False when it is completed
        SearchCommand.IsExecuting.Subscribe(isExecuting => { IsBusy = isExecuting; });

        // Subscribe to the ThrownExceptions property of the SearchCommand property
        SearchCommand.ThrownExceptions.Subscribe(ex =>
        {
            Console.WriteLine("Failed to search for shops");
            Console.WriteLine(ex.Message);
        });
    }

    private async Task DoSearch()
    {
        try
        {
            // Clear the ShopList property
            RxApp.MainThreadScheduler.Schedule(() => ShopList?.Clear());

            // Perform the search operation
            // Use the SearchText property as the accountId parameter to search for shops
            // Use the HttpClient registered in the DI container to call the API (https://localhost:7045/api/PosAdmin/shopList?accountid={accountId}
            // Add the search results to the ShopList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request =
                new HttpRequestMessage(HttpMethod.Get,
                    $"https://localhost:7045/api/PosAdmin/shopList?" +
                    $"accountid={SearchTextAccountId}&shopId={SearchTextShopId}");
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var resultShopList = JsonSerializer.Deserialize<List<Shop>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // Add the shop to the ShopList property in the UI thread using RxApp.MainThreadScheduler
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                foreach (var shop in resultShopList)
                {
                    // Add the search results to the ShopList property
                    ShopList?.Add(shop);

                    // add the first shop in the list to the SelectedShop property
                    SelectedShop = ShopList?.FirstOrDefault();
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}