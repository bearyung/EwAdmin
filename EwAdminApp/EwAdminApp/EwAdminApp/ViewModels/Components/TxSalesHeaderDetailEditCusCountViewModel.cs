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

public class TxSalesHeaderDetailEditCusCountViewModel : ViewModelBase
{
    // properties
    private TxSalesHeader? _selectedTxSalesHeader;
    private string? _cusCountStringInput;
    private bool _isBusy;

    public TxSalesHeader? SelectedTxSalesHeader
    {
        get => _selectedTxSalesHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeader, value);
    }
    
    public string? CusCountStringInput
    {
        get => _cusCountStringInput;
        set => this.RaiseAndSetIfChanged(ref _cusCountStringInput, value);
    }
    
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    // commands
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    
    // cancellationTokenSource for the async task
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public TxSalesHeaderDetailEditCusCountViewModel()
    {
        // SaveCommand can be executed if the SelectedTxSalesHeader is not null and the CusCountStringInput is a valid number 
        var canExecuteSave = this.WhenAnyValue(
            x => x.SelectedTxSalesHeader,
            x => x.CusCountStringInput,
            (selectedTxSalesHeader, cusCountStringInput) =>
                selectedTxSalesHeader != null && int.TryParse(cusCountStringInput, out _));
        
        // CancelCommand can be execute if the SelectedTxSalesHeader is not null and IsBusy is false
        var canExecuteCancel = this.WhenAnyValue(
            x => x.SelectedTxSalesHeader,
            x => x.IsBusy,
            (selectedTxSalesHeader, isBusy) => selectedTxSalesHeader != null && !isBusy);
        
        // implement the SaveCommand
        SaveCommand = ReactiveCommand.CreateFromTask(
            execute: DoSave,
            canExecute: canExecuteSave);
        
        // implement the CancelCommand
        CancelCommand = ReactiveCommand.CreateFromTask(DoCancel);
        
        this.WhenActivated(disposables =>
        {
            // log the activation
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
            
            // listen to the TxSalesHeaderEvent, and update the SelectedTxSalesHeader
            MessageBus.Current.Listen<TxSalesHeaderEvent>()
                .Subscribe(txSalesHeaderEvent =>
                {
                    // update the SelectedTxSalesHeader
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        SelectedTxSalesHeader = txSalesHeaderEvent.TxSalesHeaderMessage;
                        CusCountStringInput = SelectedTxSalesHeader?.CusCount.ToString();
                    });
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
            
            // log when the ViewModel is deactivated
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");

                    // cancel the CancellationTokenSource
                    _cancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }
    
    // implement the DoSave method
    // this method will be called when the SaveCommand is executed
    // it will save the SselectedTxSalesHeader (with a new dto)
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
                CusCount = int.Parse(CusCountStringInput ?? "0")
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
                CusCountStringInput = SelectedTxSalesHeader?.CusCount.ToString();
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
            CusCountStringInput = SelectedTxSalesHeader?.CusCount.ToString();
        });

        return Task.CompletedTask;
    }
}