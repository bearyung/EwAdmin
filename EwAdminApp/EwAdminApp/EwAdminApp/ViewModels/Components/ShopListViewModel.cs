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

public class ShopListViewModel : ViewModelBase
{
    private ObservableCollection<Shop> _shopList = [];
    private Shop? _selectedShop;
    private bool _isBusy;
    private string? _searchTextAccountId;
    private string? _searchTextShopId;
    private string? _searchTextShopNameContains;

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
    
    public string? SearchTextShopNameContains
    {
        get => _searchTextShopNameContains;
        set => this.RaiseAndSetIfChanged(ref _searchTextShopNameContains, value);
    }

    public ReactiveCommand<Unit, Unit>? SearchCommand { get; }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    public ShopListViewModel()
    {
        // Initialize the ShopList property
        ShopList = new ObservableCollection<Shop>();

        // Initialize the SearchCommand property
        SearchCommand = ReactiveCommand.CreateFromTask(DoSearch);

        this.WhenActivated((disposables) =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name}: Activated");

            this.WhenAnyValue(x => x.SelectedShop)
                .Subscribe(shop =>
                {
                    Console.WriteLine($"{GetType().Name}: SelectedShop changed: " + shop?.Name);

                    // Any other logic needed when a shop is selected
                    // emit the ShopEvent using the ReactiveUI MessageBus
                    MessageBus.Current.SendMessage(new ShopEvent(shop));
                })
                .DisposeWith(disposables);

            // set the IsBusy property to True when the SearchCommand is executing, False when it is completed
            SearchCommand.IsExecuting.Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;

                    IsBusy = isExecuting;

                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus
                    if (!isInitial && isExecuting)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = ActionStatus.StatusEnum.Executing,
                                Message = "Searching for shops..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // Subscribe to the ThrownExceptions property of the SearchCommand property
            SearchCommand.ThrownExceptions.Subscribe(ex =>
            {
                Console.WriteLine("Failed to search for shops");
                Console.WriteLine(ex.Message);

                // use MessageBus to emit an ActionStatusMessageEvent for the error operation
                MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                    new ActionStatus
                    {
                        ActionStatusEnum = ActionStatus.StatusEnum.Error,
                        Message = ex.Message
                    }));
            });

            // Subscribe to the ExecutingCommandsCount property
            this.WhenAnyValue(x => x.ExecutingCommandsCount)
                .Subscribe(count => { Console.WriteLine($"{GetType().Name}: ExecutingCommandsCount: {count}"); })
                .DisposeWith(disposables);

            // Subscribe to the SearchCommand's Executed observable
            // Subscribe to the SearchCommand itself
            SearchCommand.Subscribe(_ =>
                {
                    MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                        new ActionStatus
                        {
                            ActionStatusEnum = ActionStatus.StatusEnum.Completed,
                            Message = "Shop search completed"
                        }));
                })
                .DisposeWith(disposables);

            // console log when the viewmodel is deactivated
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");

                    // cancel the CancellationTokenSource
                    _cancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }

    private async Task DoSearch()
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

            // Clear the ShopList property
            RxApp.MainThreadScheduler.Schedule(() => ShopList.Clear());

            // Perform the search operation
            // Use the SearchText property as the accountId parameter to search for shops
            // Use the HttpClient registered in the DI container to call the API (/api/PosAdmin/shopList?accountid={accountId}
            // Add the search results to the ShopList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request =
                new HttpRequestMessage(HttpMethod.Get,
                    $"/api/PosAdmin/shopList?" +
                    $"page=1&pageSize=100&" +
                    $"accountId={SearchTextAccountId}&shopId={SearchTextShopId}&shopNameContains={SearchTextShopNameContains}");
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
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(DoSearch)} operation cancelled");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}