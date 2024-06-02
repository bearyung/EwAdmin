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

public class TableMasterListViewModel : ViewModelBase
{
    // properties
    // SelectedShop, TableMasterList, SelectedTableMaster, IsBusy
    private Shop? _selectedShop;
    private ObservableCollection<TableMaster>? _tableMasterList;
    private TableMaster? _selectedTableMaster;
    private bool _isBusy;
    
    // properties for the search parameters
    
    private string? _searchTextTableId;
    private string? _searchTextTableCode;
    private bool _showDisabled;
    private bool _showEnabled;
    private bool _showTempTable;
    private bool _showTakeAway;
    private bool _showDineIn;
    
    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }
    
    public ObservableCollection<TableMaster>? TableMasterList
    {
        get => _tableMasterList;
        set => this.RaiseAndSetIfChanged(ref _tableMasterList, value);
    }
    
    public TableMaster? SelectedTableMaster
    {
        get => _selectedTableMaster;
        set => this.RaiseAndSetIfChanged(ref _selectedTableMaster, value);
    }
    
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    public string? SearchTextTableId
    {
        get => _searchTextTableId;
        set => this.RaiseAndSetIfChanged(ref _searchTextTableId, value);
    }
    
    public string? SearchTextTableCode
    {
        get => _searchTextTableCode;
        set => this.RaiseAndSetIfChanged(ref _searchTextTableCode, value);
    }
    
    public bool ShowDisabled
    {
        get => _showDisabled;
        set => this.RaiseAndSetIfChanged(ref _showDisabled, value);
    }
    
    public bool ShowEnabled
    {
        get => _showEnabled;
        set => this.RaiseAndSetIfChanged(ref _showEnabled, value);
    }
    
    public bool ShowTempTable
    {
        get => _showTempTable;
        set => this.RaiseAndSetIfChanged(ref _showTempTable, value);
    }
    
    public bool ShowTakeAway
    {
        get => _showTakeAway;
        set => this.RaiseAndSetIfChanged(ref _showTakeAway, value);
    }
    
    public bool ShowDineIn
    {
        get => _showDineIn;
        set => this.RaiseAndSetIfChanged(ref _showDineIn, value);
    }
    
    // add a SearchCommand for DoSearch
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    
    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public TableMasterListViewModel()
    {
        // Create an observable that evaluates whether SearchCommand can execute
        // SearchCommand can be executed if SelectedShop is not null and IsBusy is false
        var canExecuteSearch = this.WhenAnyValue(
            x => x.SelectedShop,
            x => x.IsBusy,
            (selectedShop, isBusy) => selectedShop != null && !isBusy);
        
        // implement the SearchCommand
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);
        
        this.WhenActivated(disposables =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions.Subscribe(ex =>
            {
                Console.WriteLine("Failed to search for TableMaster.");
                Console.WriteLine(ex);
                
                MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                    sourceTypeName: GetType().Name, 
                    isExecutionIncrement: false));
            });
            
            // set isBusy to true when the SearchCommand is executing
            // also set the ExecutingCommandsCount property to plus 1
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
                                Message = "Searching for TableMaster list..."
                            }));
                    }
                })
                .DisposeWith(disposables);
            
            // subscribe to the SelectedTableMaster property
            // emit the TableMasterEvent using the ReactiveUI MessageBus
            this.WhenAnyValue(x => x.SelectedTableMaster)
                .Subscribe(x =>
                {
                    MessageBus.Current.SendMessage(new TableMasterEvent(x));
                })
                .DisposeWith(disposables);
            
            // subscribe to the ShopEvent to update the SelectedShop property
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(x =>
                {
                    // console log the event received from the ShopEvent in this ViewModel
                    // need to include the viewmodel name, and the shop name
                    Console.WriteLine($"{GetType().Name}: ShopEvent received: " + x.ShopMessage?.Name);

                    // set the SelectedShop property to the Shop property in the ShopEvent
                    SelectedShop = x.ShopMessage;
                    
                    // clear the TableMasterList and SelectedTableMaster property
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        TableMasterList?.Clear();
                        SelectedTableMaster = null;
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
                            Message = "TableMaster list search completed."
                        }));
                })
                .DisposeWith(disposables);
            
            // log when the viewmodel is deactivated
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
    // It sends a request to the server to get the TableMaster list
    // The request is sent to the API:
    // /api/PosAdmin/tableList?accountId={accountId}&shopId={shopId}&showDisabled=true&showEnabled=true&showTempTable=true&showTakeAway=true&showDineIn=true&tableId={tableId}&tableCode={tableCode}
    // The response is deserialized into a list of TableMaster objects
    // The list is added to the TableMasterList property
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
            
            // Clear the TableMasterList property
            RxApp.MainThreadScheduler.Schedule(() => TableMasterList?.Clear());

            // Perform the search operation
            // Use the HttpClient registered in the DI container to call the API:
            // // /api/PosAdmin/tableList?accountId={accountId}&shopId={shopId}&showDisabled=true&showEnabled=true&showTempTable=true&showTakeAway=true&showDineIn=true&tableId={tableId}&tableCode={tableCode}
            // Add the search results to the TableMasterList property
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request =
                new HttpRequestMessage(HttpMethod.Get,
                    $"/api/PosAdmin/tableList?accountId={SelectedShop?.AccountId}&shopId={SelectedShop?.ShopId}&showDisabled={ShowDisabled}&showEnabled={ShowEnabled}&showTempTable={ShowTempTable}&showTakeAway={ShowTakeAway}&showDineIn={ShowDineIn}&tableId={SearchTextTableId}&tableCode={SearchTextTableCode}");
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
            var resultTableMasterList = JsonSerializer.Deserialize<List<TableMaster>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // Add the search results to the TableMasterList property
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                TableMasterList?.Clear();

                foreach (var tableMasterObj in resultTableMasterList)
                {
                    // Add the tableMaster to the TableMasterList property in the UI thread using RxApp.MainThreadScheduler
                    TableMasterList?.Add(tableMasterObj);

                    // add the first shop in the list to the SelectedTableMaster property
                    SelectedTableMaster = resultTableMasterList?.FirstOrDefault();
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