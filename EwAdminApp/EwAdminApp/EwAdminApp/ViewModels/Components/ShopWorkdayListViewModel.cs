using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

public class ShopWorkdayDetailListViewModel : ViewModelBase
{
    // using ReactiveUI for all implementations
    // Add a properties which has referenced from ShopWorkdayListView.axaml
    private Shop? _selectedShop;

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    // Add a command for SearchTextBox_OnKeyDown
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a properties IsBusy
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a string property SearchText
    private string? _searchText
        = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    // add a property for ObservableCollection ShopWorkdayDetail List
    private ObservableCollection<ShopWorkdayDetail>? _shopWorkdayDetailList;

    public ObservableCollection<ShopWorkdayDetail>? ShopWorkdayDetailList
    {
        get => _shopWorkdayDetailList;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayDetailList, value);
    }

    // SelectedShopWorkdayDetail property
    private ShopWorkdayDetail? _selectedShopWorkdayDetail;

    public ShopWorkdayDetail? SelectedShopWorkdayDetail
    {
        get => _selectedShopWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayDetail, value);
    }
    
    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    public ShopWorkdayDetailListViewModel()
    {
        // Initialize the ShopWorkdayDetailList
        ShopWorkdayDetailList = new ObservableCollection<ShopWorkdayDetail>();

        // Create an observable that evaluates whether SearchCommand can execute
        var canExecuteSearch = this.WhenAnyValue(x => x.SelectedShop)
            .Select(x => x != null);

        // Implement the SearchShopWorkdayDetail command
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);


        this.WhenActivated(disposables =>
        {
            // console log when the ViewModel is activated
            Console.WriteLine($"{GetType().Name} is now active.");
            
            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for shop workday detail");
                    Console.WriteLine(ex.Message);
                    
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // set isBusy to true when the SearchCommand is executing, false when it is completed
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
                                Message = "Searching for shop workday detail list..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // subscribe to the SelectedShopWorkdayDetail property
            // emit the ShopWorkdayDetailEvent using the ReactiveUI MessageBus when the SelectedShopWorkdayDetail property changes
            this.WhenAnyValue(x => x.SelectedShopWorkdayDetail)
                .Subscribe(shopWorkdayDetail =>
                {
                    // emit the ShopWorkdayDetailEvent using the ReactiveUI MessageBus
                    MessageBus.Current.SendMessage(new ShopWorkdayDetailEvent(shopWorkdayDetail));
                })
                .DisposeWith(disposables);

            // Subscribe to the ShopEvent to update the SelectedShop property
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(shopEvent =>
                {
                    // console log the event received from the ShopEvent in this ViewModel
                    // need to include the viewmodel name, and the shop name
                    Console.WriteLine($"{GetType().Name}: ShopEvent received: " + shopEvent.ShopMessage?.Name);

                    // set the SelectedShop property to the Shop property in the ShopEvent
                    SelectedShop = shopEvent.ShopMessage;

                    // clear the searchText property
                    // SearchText = string.Empty;

                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        // clear the ShopWorkdayDetailList property
                        ShopWorkdayDetailList?.Clear();

                        // clear the SelectedShopWorkdayDetail property
                        SelectedShopWorkdayDetail = null;
                    });
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
                            Message = "Shop workday detail search completed"
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

    // Implement the async Search ShopWorkdayDetail method
    // This method will be called when the SearchCommand is executed
    // This method will search the ShopWorkdayDetail based on the SearchText and SelectedShop
    // The result will be assigned to the ShopWorkdayDetailList
    // API Endpoint to use: /api/PosAdmin/shopworkdaydetaillist?accountid={accountId}&shopid={shopId}
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
            
            // Throw OperationCanceledException if the operation is cancelled
            cancellationToken.ThrowIfCancellationRequested();
            
            // Clear the ShopWorkdayDetailList property
            RxApp.MainThreadScheduler.Schedule(() => ShopWorkdayDetailList?.Clear());

            // Perform the search operation
            // Use the HttpClient registered in the DI container to call the API:
            // /api/PosAdmin/shopworkdaydetaillist?accountid={SelectedShop.AccountId}&shopid={SelectedShop.ShopId}&startDate={SearchText}
            // Add the search results to the ShopWorkdayDetailList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;
            
            // create the parameters of startDate, endDate from SearchText for the request
            // input format of SearchText is "2024-01-01:2024-11-30" or "2024-12-01" or "2024-05-01:" or ":2024-12-31"
            var startDate = "";
            var endDate = "";
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchTextArrl = SearchText.Split(':');
                if (searchTextArrl.Length == 1)
                {
                    // case of ":2024-12-31", user want date less than or equal to 2024-12-31
                    if (searchTextArrl[0].StartsWith(":"))
                    {
                        endDate = searchTextArrl[0].Substring(1);
                    }
                    // case of "2024-05-01:", user want date greater than or equal to 2024-05-01
                    else if (searchTextArrl[0].EndsWith(":"))
                    {
                        startDate = searchTextArrl[0];
                    }
                    // case of "2024-12-01", user want exact date 2024-12-01
                    else
                    {
                        startDate = searchTextArrl[0];
                        endDate = searchTextArrl[0];
                    }
                }
                // case of "1000:2000", user want amount between 1000 and 2000 (inclusive)
                else if (searchTextArrl.Length == 2)
                {
                    startDate = searchTextArrl[0];
                    endDate = searchTextArrl[1];
                }
            }

            var request =
                new HttpRequestMessage(HttpMethod.Get,
                    $"/api/posAdmin/shopWorkdayDetailList?accountId={SelectedShop?.AccountId}" +
                    $"&shopId={SelectedShop?.ShopId}&startDate={startDate}&endDate={endDate}");
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

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
            var resultShopWorkdayDetailList = JsonSerializer.Deserialize<List<ShopWorkdayDetail>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // Add the search results to the ShopList property
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ShopWorkdayDetailList?.Clear();

                foreach (var shopWorkdayDetail in resultShopWorkdayDetailList)
                {
                    // Add the shop to the ShopList property in the UI thread using RxApp.MainThreadScheduler
                    ShopWorkdayDetailList?.Add(shopWorkdayDetail);

                    // add the first shop in the list to the SelectedShop property
                    SelectedShopWorkdayDetail = resultShopWorkdayDetailList?.FirstOrDefault();
                }
            });
        }
        catch(OperationCanceledException)
        {
            // log the operation cancelled
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