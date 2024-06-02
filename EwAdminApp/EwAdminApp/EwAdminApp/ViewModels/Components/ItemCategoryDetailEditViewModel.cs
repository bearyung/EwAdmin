using System;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class ItemCategoryDetailEditViewModel : ViewModelBase
{
    private ItemCategory? _selectedItemCategory;

    public ItemCategory? SelectedItemCategory
    {
        get => _selectedItemCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedItemCategory, value);
    }

    // add a clone of the selected item category
    private ItemCategory? _selectedItemCategoryClone;

    public ItemCategory? SelectedItemCategoryClone
    {
        get => _selectedItemCategoryClone;
        set => this.RaiseAndSetIfChanged(ref _selectedItemCategoryClone, value);
    }

    // add a reactive SaveCommand
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    // add a reactive CancelCommand
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // add an IsBusy property
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // add a constructor
    public ItemCategoryDetailEditViewModel()
    {
        var canSave = this.WhenAnyValue(
            x => x.SelectedItemCategory,
            x => x.IsBusy,
            (selectedItemCategory, isBusy) => selectedItemCategory != null && !isBusy);

        var canCancel = this.WhenAnyValue(
            x => x.SelectedItemCategoryClone,
            x => x.IsBusy,
            (selectedItemCategoryClone, isBusy) => selectedItemCategoryClone != null && !isBusy);

        // initialize the SaveCommand and CancelCommand
        SaveCommand = ReactiveCommand.CreateFromTask(DoSave, canSave);
        CancelCommand = ReactiveCommand.CreateFromTask(DoCancel, canCancel);

        this.WhenActivated(disposables =>
        {
            // console log the activated message
            Console.WriteLine($"{GetType().Name}: Activated");

            // when the SaveCommand is executing, set the IsBusy to true
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
                                Message = "Saving item category..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // handle the exceptions thrown by the SaveCommand
            SaveCommand.ThrownExceptions.Subscribe(ex =>
                {
                    MessageBus.Current.SendMessage(new ExecutingCommandFlagEvent( 
                        sourceTypeName: GetType().Name, 
                        isExecutionIncrement: false));
                    
                    // log the exception
                    Console.WriteLine($"An error occurred: {ex.Message}");
                })
                .DisposeWith(disposables);
            
            // listen to the messagebus for the ItemCategoryEvent
            // when the ItemCategoryEvent is received, set the SelectedItemCategoryClone to the SelectedItemCategory
            MessageBus.Current.Listen<ItemCategoryEvent>()
                .Subscribe(itemCategoryEvent =>
                {
                    var serializedItemCategory = 
                        JsonSerializer.Serialize(itemCategoryEvent.ItemCategoryMessage);
                    
                    SelectedItemCategoryClone = 
                        JsonSerializer.Deserialize<ItemCategory>(serializedItemCategory);
                    
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        SelectedItemCategory = itemCategoryEvent.ItemCategoryMessage;
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
                            Message = "Item category saved successfully"
                        }));
                })
                .DisposeWith(disposables);

            // console log the deactivated message
            Disposable
                .Create(() =>
                {
                    Console.WriteLine($"{GetType().Name}: Deactivated");
                    
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
            // cancel the previous operation if it is still running
            await _cancellationTokenSource.CancelAsync();

            // create a new cancellation token source
            _cancellationTokenSource = new CancellationTokenSource();

            // get the cancellation token
            var cancellationToken = _cancellationTokenSource.Token;

            // throw an OperationCanceledException if the cancellation token is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // Save the selected item category
            // HTTP PATCH request to the API to save the selected item category
            // API endpoint: /api/PosAdmin/updateItemCategory
            // Request body: SelectedItemCategory
            // Request headers: Authorization
            // Response: ItemCategory
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            // check if the SelectedItemCategory is null
            if (SelectedItemCategory == null) return;

            var requestItemCategory = new ItemCategory
            {
                AccountId = SelectedItemCategory.AccountId,
                CategoryId = SelectedItemCategory.CategoryId,
                CategoryTypeId = SelectedItemCategory.CategoryTypeId,
                ParentCategoryId = SelectedItemCategory.ParentCategoryId,
                IsTerminal = SelectedItemCategory.IsTerminal,
                Enabled = SelectedItemCategory.Enabled
            };

            var request = new HttpRequestMessage(HttpMethod.Patch,
                "api/PosAdmin/updateItemCategory")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestItemCategory), Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentLoginSettings.ApiKey);

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
            var resultItemCategory = JsonSerializer.Deserialize<ItemCategory>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // send a ItemCategoryEvent to the message bus
            MessageBus.Current.SendMessage(new ItemCategoryEvent(resultItemCategory));

            // log the success message
            Console.WriteLine("Item category saved successfully.");
        }
        catch (OperationCanceledException)
        {
            // log the operation cancelled
            Console.WriteLine($"{nameof(DoSave)} operation cancelled.");
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
        // reset the selected item category to the clone
        // serialize the clone to a json string and then deserialize it back to an ItemCategory object
        if (SelectedItemCategoryClone == null) return Task.CompletedTask;

        var selectedItemCategoryCloneJson = JsonSerializer.Serialize(SelectedItemCategoryClone);
        var selectedItemCategoryCloneObj =
            JsonSerializer.Deserialize<ItemCategory>(selectedItemCategoryCloneJson);

        RxApp.MainThreadScheduler.Schedule(() => SelectedItemCategory = selectedItemCategoryCloneObj);

        return Task.CompletedTask;
    }
}