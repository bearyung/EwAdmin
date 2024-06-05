using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DialogHostAvalonia;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using EwAdminApp.Models;
using EwAdminApp.ViewModels.Dialogs;
using EwAdminApp.Views;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class TxSalesHeaderDetailEditTableViewModel : ViewModelBase
{
    // properties for the view
    // selectedShop, selectedTableMaster, selectedTxSalesHeader
    // code here

    private Shop? _selectedShop;
    private TableMaster? _selectedTableMaster;
    private TxSalesHeader? _selectedTxSalesHeader;
    private bool _isBusy;

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    public TableMaster? SelectedTableMaster
    {
        get => _selectedTableMaster;
        set => this.RaiseAndSetIfChanged(ref _selectedTableMaster, value);
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

    // add the ReactiveCommand property
    // SelectTableCommand
    public ReactiveCommand<Unit, Task> SelectTableCommand { get; }

    // SaveCommand
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // cancellationTokenSource for the async task
    private CancellationTokenSource _cancellationTokenSource = new();

    // add the constructor
    // code here
    public TxSalesHeaderDetailEditTableViewModel()
    {
        // SelectTableCommand can execute when SelectedShop and SelectedTxSalesHeader are not null and IsBusy is false
        var canExecuteSelectTableCommand = this.WhenAnyValue(
            x => x.SelectedShop,
            x => x.SelectedTxSalesHeader,
            x => x.IsBusy,
            (shop, txSalesHeader, isBusy) => shop != null && txSalesHeader != null && !isBusy);

        // SaveCommand can execute when SelectedTableMaster and SelectedTxSalesHeader are not null
        var canExecuteSaveCommand = this.WhenAnyValue(
            x => x.SelectedTableMaster,
            x => x.SelectedTxSalesHeader,
            (tableMaster, txSalesHeader) => tableMaster != null && txSalesHeader != null);

        // CancelCommand can execute when SelectedTxSalesHeader, SelectedTableMaster are not null and IsBusy is false
        var canExecuteCancelCommand = this.WhenAnyValue(
            x => x.SelectedTxSalesHeader,
            x => x.SelectedTableMaster,
            x => x.IsBusy,
            (txSalesHeader, tableMaster, isBusy) => txSalesHeader != null && tableMaster != null && !isBusy);


        // initialize the SelectTableCommand property with a new ReactiveCommand
        // code here
        SelectTableCommand = ReactiveCommand.Create(async () =>
        {
            // popup a viewmodel to select a table using DialogHost.Avalonia
            // DialogHost.Show(new TableMasterListViewModel());
            var result = await DialogHost.Show(new TableMasterListDialogViewModel(SelectedShop),
                delegate(object sender, DialogClosingEventArgs _)
                {
                    if (sender is DialogHost
                        {
                            DialogContent: TableMasterListDialogViewModel { IsCancelled: false } vm
                        })
                    {
                        SelectedTableMaster = vm.SelectedTableMaster;
                    }
                });
        }, canExecuteSelectTableCommand);

        // initialize the SaveCommand property with a new ReactiveCommand
        SaveCommand = ReactiveCommand.CreateFromTask(DoSave, canExecuteSaveCommand);
        // initialize the CancelCommand property with a new ReactiveCommand
        CancelCommand = ReactiveCommand.CreateFromTask(DoCancel, canExecuteCancelCommand);

        this.WhenActivated(disposables =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");

            // set the IsBusy to true when the SaveCommand is executing
            SaveCommand.IsExecuting
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
                                Message = "Saving TxSalesHeader..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // Subscribe to the SaveCommand's Executed observable
            // Subscribe to the SaveCommand itself
            SaveCommand.Subscribe(_ =>
                {
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent(
                        sourceTypeName: GetType().Name,
                        isExecutionIncrement: false));

                    MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                        new ActionStatus
                        {
                            ActionStatusEnum = ActionStatus.StatusEnum.Completed,
                            Message = "TxSalesHeader saved"
                        }));
                })
                .DisposeWith(disposables);

            // handle the exception when the SaveCommand is executed
            SaveCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to save TxSalesHeader");
                    Console.WriteLine(ex.Message);

                    MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                        new ActionStatus
                        {
                            ActionStatusEnum = ActionStatus.StatusEnum.Error,
                            Message = "Failed to save TxSalesHeader"
                        }));

                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent(
                        sourceTypeName: GetType().Name,
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // listen to the Message Bus for the TxSalesHeaderEvent
            // when the event is received, update the SelectedTxSalesHeader property
            // code here
            MessageBus.Current.Listen<TxSalesHeaderEvent>()
                .Subscribe(x =>
                {
                    // log the message
                    Console.WriteLine($"{GetType()} Received TxSalesHeaderEvent: " +
                                      $"{x.TxSalesHeaderMessage?.TxSalesHeaderId}");

                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        // update the SelectedTxSalesHeader property
                        SelectedTxSalesHeader = x.TxSalesHeaderMessage;
                    });
                })
                .DisposeWith(disposables);

            // listen to the message bus for the ShopEvent
            // when the event is received, update the SelectedShop property
            // code here
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(x =>
                {
                    // log the message
                    Console.WriteLine($"{GetType().Name} Received ShopEvent: " +
                                      $"{x.ShopMessage?.ShopId} {x.ShopMessage?.Name}");

                    // update the SelectedShop property
                    SelectedShop = x.ShopMessage;
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

    private async Task DoSave()
    {
        try
        {
            // Cancel the previous save operation
            await _cancellationTokenSource.CancelAsync();

            // Create a new CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();

            // Get the cancellation token from the CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token;

            // Throw an OperationCanceledException if the CancellationToken is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // Save the SelectedTxSalesHeader by calling the API endpoint: /api/PosAdmin/updateTxSalesHeader
            // Request method: PUT
            // Request header: Authorization with the token (Bearer token)
            // Request body: TxSalesHeader with only the fields that need to be updated (accountId, shopId, txSalesHeaderId, cusCount, tableId, tableCode, sectionId, sectionName, Enabled)
            // Response: TxSalesHeader

            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            if (SelectedTxSalesHeader == null) return;

            var requestTxSalesHeader = new
            {
                AccountId = SelectedTxSalesHeader.AccountId,
                ShopId = SelectedTxSalesHeader.ShopId,
                TxSalesHeaderId = SelectedTxSalesHeader.TxSalesHeaderId,
                TableId = SelectedTableMaster?.TableId,
                TableCode = SelectedTableMaster?.TableCode,
                SectionId = SelectedTableMaster?.SectionId,
                SectionName = SelectedTableMaster?.SectionName,
            };

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/PosAdmin/updateTxSalesHeader")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestTxSalesHeader), System.Text.Encoding.UTF8,
                    "application/json")
            };

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

            // update the SelectedTxSalesHeader with the resultTxSalesHeader
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                SelectedTxSalesHeader = resultTxSalesHeader;
                SelectedTableMaster = null;
            });

            // user the MessageBus to publish the TxSalesHeaderEvent
            MessageBus.Current.SendMessage(new TxSalesHeaderEvent(resultTxSalesHeader));

            // Log the success message
            Console.WriteLine($"Successfully saved {nameof(TxSalesHeader)}");
        }
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(DoSave)} operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            // Log the exception message
            Console.WriteLine($"Failed to save {nameof(TxSalesHeader)}");
            Console.WriteLine(ex.Message);

            throw;
        }
    }

    private Task DoCancel()
    {
        // reset the CusCountStringInput to the original value of the SelectedTxSalesHeader
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            RxApp.MainThreadScheduler.Schedule(() => { SelectedTableMaster = null; });
        });

        return Task.CompletedTask;
    }
}