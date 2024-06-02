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

public class ShopWorkdayPeriodDetailEditViewModel : ViewModelBase
{
    // this viewmodel is used to edit the details of a ShopWorkdayPeriodDetail
    // add a property named "SelectedShopWorkdayPeriodDetail" of type ShopWorkdayPeriodDetail
    // code here
    private ShopWorkdayPeriodDetail? _selectedShopWorkdayPeriodDetail;

    public ShopWorkdayPeriodDetail? SelectedShopWorkdayPeriodDetail
    {
        get => _selectedShopWorkdayPeriodDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayPeriodDetail, value);
    }

    // add a property named "SelectedShopWorkdayPeriodDetailClone" of type ShopWorkdayPeriodDetail
    // code here
    private ShopWorkdayPeriodDetail? _selectedShopWorkdayPeriodDetailClone;

    public ShopWorkdayPeriodDetail? SelectedShopWorkdayPeriodDetailClone
    {
        get => _selectedShopWorkdayPeriodDetailClone;
        set => this.RaiseAndSetIfChanged(ref _selectedShopWorkdayPeriodDetailClone, value);
    }

    // add a SaveCommand property of type ReactiveCommand<Unit, Unit>
    // code here
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    // add a CancelCommand property of type ReactiveCommand<Unit, Unit>
    // code here
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // add a IsBusy property of type bool
    // code here
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // add a constructor
    // code here
    public ShopWorkdayPeriodDetailEditViewModel()
    {
        // Create an observable that evaluates whether SaveCommand can execute
        // SaveCommand can be executed if SelectedShopWorkdayPeriodDetail is not null and IsBusy is false
        var canSave = this.WhenAnyValue(
            x => x.SelectedShopWorkdayPeriodDetail,
            x => x.IsBusy,
            (selectedShopWorkdayPeriodDetail, isBusy) => selectedShopWorkdayPeriodDetail != null && !isBusy);

        var canCancel = this.WhenAnyValue(
            x => x.IsBusy,
            isBusy => !isBusy);

        // initialize the SaveCommand
        SaveCommand = ReactiveCommand.CreateFromTask(DoSave, canSave);

        // initialize the CancelCommand
        CancelCommand = ReactiveCommand.CreateFromTask(DoCancel, canCancel);

        this.WhenActivated((disposables) =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} is now active.");
            
            // when the SaveCommand is executing, set the IsBusy property to true
            SaveCommand.IsExecuting.Subscribe(isExecuting =>
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
                                Message = "Saving ShopWorkdayPeriodDetail..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // handle the exception when the SaveCommand is executed
            SaveCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                })
                .DisposeWith(disposables);

            // listen to the Message Bus for the ShopWorkdayPeriodDetailEvent
            // when the event is received, set the SelectedShopWorkdayPeriodDetail property
            MessageBus.Current.Listen<ShopWorkdayPeriodDetailEvent>()
                .Subscribe(shopWorkdayPeriodDetailEvent =>
                {
                    // make a copy of the SelectedShopWorkdayPeriodDetail and set it to the SelectedShopWorkdayPeriodDetailClone
                    var serializedShopWorkdayPeriodDetail =
                        JsonSerializer.Serialize(shopWorkdayPeriodDetailEvent.ShopWorkdayPeriodDetailMessage);
                    SelectedShopWorkdayPeriodDetailClone =
                        JsonSerializer.Deserialize<ShopWorkdayPeriodDetail>(serializedShopWorkdayPeriodDetail);

                    // set the SelectedShopWorkdayPeriodDetail property using RxApp.MainThreadScheduler
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        SelectedShopWorkdayPeriodDetail =
                            shopWorkdayPeriodDetailEvent.ShopWorkdayPeriodDetailMessage;
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
                            Message = "ShopWorkdayPeriodDetail saved successfully"
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

    // add a DoSave method
    // code here
    private async Task DoSave()
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
            
            // This method is used to save the changes made to the ShopWorkdayPeriodDetail
            // get the LoginSettings from Locator.Current.GetService<LoginSettings>()
            // get the HttpClient from Locator.Current.GetService<HttpClient>()
            // Call the http PATCH API to save the changes
            // API URL: /api/PosAdmin/updateShopWorkdayPeriodDetail
            // request header: Authorization = "Bearer " + LoginSettings.ApiKey
            // request body: ShopWorkdayPeriodDetail
            // code here
            var loginSettings = Locator.Current.GetService<LoginSettings>();
            var httpClient = Locator.Current.GetService<HttpClient>();

            if (loginSettings == null || httpClient == null) return;

            // check if the SelectedShopWorkdayPeriodDetail is null
            if (SelectedShopWorkdayPeriodDetail == null) return;

            var requeestShopWorkdayPeriodDetail = new ShopWorkdayPeriodDetail
            {
                AccountId = SelectedShopWorkdayPeriodDetail.AccountId,
                ShopId = SelectedShopWorkdayPeriodDetail.ShopId,
                WorkdayPeriodDetailId = SelectedShopWorkdayPeriodDetail.WorkdayPeriodDetailId,
                WorkdayPeriodId = SelectedShopWorkdayPeriodDetail.WorkdayPeriodId,
                StartDateTime = SelectedShopWorkdayPeriodDetail.StartDateTime,
                EndDateTime = SelectedShopWorkdayPeriodDetail.EndDateTime,
                Enabled = SelectedShopWorkdayPeriodDetail.Enabled
            };

            var request = new HttpRequestMessage(HttpMethod.Patch, "/api/PosAdmin/updateShopWorkdayPeriodDetail")
            {
                Content = new StringContent(JsonSerializer.Serialize(requeestShopWorkdayPeriodDetail),
                    System.Text.Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {loginSettings.ApiKey}");

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

            var resultShopWorkdayPeriodDetail = JsonSerializer.Deserialize<ShopWorkdayPeriodDetail>(content,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // send a ShopWorkdayPeriodDetailSavedEvent using the MessageBus
            MessageBus.Current.SendMessage(new ShopWorkdayPeriodDetailEvent(resultShopWorkdayPeriodDetail));

            // log the success message
            Console.WriteLine("ShopWorkdayPeriodDetail saved successfully");
        }
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(DoSave)} operation cancelled");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Task DoCancel()
    {
        // reset the SelectedShopWorkdayPeriodDetail to the SelectedShopWorkdayPeriodDetailClone
        // serialize the SelectedShopWorkdayPeriodDetailClone and set it to the SelectedShopWorkdayPeriodDetail
        // code here
        if (SelectedShopWorkdayPeriodDetailClone == null) return Task.CompletedTask;

        var selectedShopWorkdayPeriodDetailCloneJson = JsonSerializer.Serialize(SelectedShopWorkdayPeriodDetailClone);
        var selectedShopWorkdayPeriodDetailCloneObj =
            JsonSerializer.Deserialize<ShopWorkdayPeriodDetail>(selectedShopWorkdayPeriodDetailCloneJson);

        RxApp.MainThreadScheduler.Schedule(() =>
        {
            SelectedShopWorkdayPeriodDetail = selectedShopWorkdayPeriodDetailCloneObj;
        });

        return Task.CompletedTask;
    }
}