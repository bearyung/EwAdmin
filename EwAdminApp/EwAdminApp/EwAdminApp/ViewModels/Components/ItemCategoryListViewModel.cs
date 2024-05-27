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
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class ItemCategoryListViewModel : ViewModelBase
{
    // add private fields
    private ObservableCollection<ItemCategory> _itemCategoryList = [];
    private ItemCategory? _selectedItemCategory;
    private bool _isBusy;
    private string? _searchText;
    private bool _showEnabledRecords = true;
    private bool _showDisabledRecords;
    private string? _searchTextLastModifiedDateTime
        = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private BrandMaster? _selectedBrand;

    // add public properties
    public ObservableCollection<ItemCategory> ItemCategoryList
    {
        get => _itemCategoryList;
        set => this.RaiseAndSetIfChanged(ref _itemCategoryList, value);
    }

    public ItemCategory? SelectedItemCategory
    {
        get => _selectedItemCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedItemCategory, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool ShowEnabledRecords
    {
        get => _showEnabledRecords;
        set => this.RaiseAndSetIfChanged(ref _showEnabledRecords, value);
    }

    public bool ShowDisabledRecords
    {
        get => _showDisabledRecords;
        set => this.RaiseAndSetIfChanged(ref _showDisabledRecords, value);
    }

    public string? SearchTextLastModifiedDateTime
    {
        get => _searchTextLastModifiedDateTime;
        set => this.RaiseAndSetIfChanged(ref _searchTextLastModifiedDateTime, value);
    }

    public BrandMaster? SelectedBrand
    {
        get => _selectedBrand;
        set => this.RaiseAndSetIfChanged(ref _selectedBrand, value);
    }

    // add a SearchCommand property
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    public ItemCategoryListViewModel()
    {
        // Create an observable that evaluates whether SearchCommand can execute
        // only can execute when the SelectedBrand property is not null
        var canExecuteSearch = this.WhenAnyValue(x => x.SelectedBrand)
            .Select(x => x != null);
        
        // Initialize the SearchCommand property with a new ReactiveCommand
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch, 
            canExecute: canExecuteSearch);
        
        this.WhenActivated(disposables =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // handle the exception thrown by the SearchCommand
            SearchCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.Write("Failed to search for item categories.");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);
            
            // set the IsBusy property to true when the SearchCommand is executing
            SearchCommand.IsExecuting
                .Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;
                    
                    IsBusy = isExecuting;
                    
                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus only if it is not the initial execution
                    if (!isInitial && isExecuting)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = ActionStatus.StatusEnum.Executing,
                                Message = "Searching for item categories..."
                            }));
                    }
                })
                .DisposeWith(disposables);
            
            // subscribe to the SelectedItemCategory property
            // emit the ItemCategoryEvent using the ReactiveUI MessageBus
            this.WhenAnyValue(x => x.SelectedItemCategory)
                .Subscribe(itemCategory =>
                {
                    MessageBus.Current.SendMessage(new ItemCategoryEvent(itemCategory));
                })
                .DisposeWith(disposables);
            
            // subscribe to the BrandMasterEvent using the ReactiveUI MessageBus
            // update the SelectedBrand property
            MessageBus.Current.Listen<WebAdminBrandEvent>()
                .Subscribe(brandMasterEvent =>
                {
                    // console log the event received from the MessageBus
                    Console.WriteLine($"{GetType().Name}: BrandMasterEvent received: {brandMasterEvent.BrandMessage?.BrandId}");
                    
                    // update the SelectedBrand property
                    SelectedBrand = brandMasterEvent.BrandMessage;
                    
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        // clear the ItemCategoryList property
                        ItemCategoryList.Clear();
                    
                        // clear the SelectedItemCategory property
                        SelectedItemCategory = null;
                    });
                })
                .DisposeWith(disposables);
            
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
                            Message = "Item category search completed"
                        }));
                })
                .DisposeWith(disposables);
            
            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    // Implement the DoSearch method
    // this method will be called when the SearchCommand is executed
    // this method will be responsible for searching for item categories
    // the result of the search will be stored in the ItemCategoryList property
    // the search will be based on the values of the SearchText, ShowEnabledRecords, ShowDisabledRecords, and SearchTextLastModifiedDateTime properties
    // the search will be performed asynchronously
    // API Endpoint to use: GET /api/PosAdmin/itemCategoryList?accountid=11377&page=0&pageSize=100&searchText=&showEnabledRecords=true&showDisabledRecords=false&lastModifiedDateTime=
    private async Task DoSearch()
    {
        try
        {
            // Cancel any previous search operation
            await _cancellationTokenSource.CancelAsync();

            // Create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // Get the CancellationToken from the CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token;

            // Throw an OperationCanceledException if the CancellationToken is already canceled
            cancellationToken.ThrowIfCancellationRequested();

            // clear the ItemCategoryList property
            RxApp.MainThreadScheduler.Schedule(() => ItemCategoryList.Clear());

            // perform the search operation
            // use the HTTPClient registered in the Splat container to make the API request
            // get the current login settings from the Locator
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            // get the HTTPClient from the Locator
            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            // create a new HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/itemCategoryList?" +
                $"accountid={SelectedBrand?.BrandId}&" +
                $"page=1&" +
                $"pageSize=100&" +
                $"categoryNameContains={SearchText}&" +
                $"showEnabledRecords={ShowEnabledRecords}&" +
                $"showDisabledRecords={ShowDisabledRecords}&" +
                $"lastModifiedDateTime={SearchTextLastModifiedDateTime}");
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            // send the request and get the response
            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // check if the response is successful
            if (!response.IsSuccessStatusCode)
            {
                // log the error
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Error: {errorContent}");
                
                // throw an exception with error code and content
                throw new Exception($"Error: {response.StatusCode} - {errorContent}");
            }

            // read the response content
            var content = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            // parse the response content to a list of ItemCategory objects
            var resultItemCategoryList = JsonSerializer.Deserialize<List<ItemCategory>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // update the ItemCategoryList property with the result of the search
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ItemCategoryList.Clear();

                foreach (var itemCategory in resultItemCategoryList)
                {
                    ItemCategoryList?.Add(itemCategory);

                    // set the first item category in the list as the selected item category
                    SelectedItemCategory = ItemCategoryList?.FirstOrDefault();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // log the cancellation of the search operation
            Console.WriteLine($"{nameof(DoSearch)} operation cancelled");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}