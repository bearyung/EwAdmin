using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public class ShopWorkdayPeriodDetailListViewModel : ViewModelBase
{
    // this viewmodel is used to display the list of ShopWorkdayPeriodDetails
    // add a property named "ShopWorkdayPeriodDetailList" of type ObservableCollection<ShopWorkdayPeriodDetail>
    private ObservableCollection<ShopWorkdayPeriodDetail> _shopWorkdayPeriodDetailList = [];

    public ObservableCollection<ShopWorkdayPeriodDetail> ShopWorkdayPeriodDetailList
    {
        get => _shopWorkdayPeriodDetailList;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayPeriodDetailList, value);
    }

    // add a SelectedWorkdayPeriodDetail property of type ShopWorkdayPeriodDetail
    private ShopWorkdayPeriodDetail? _selectedWorkdayPeriodDetail;

    public ShopWorkdayPeriodDetail? SelectedWorkdayPeriodDetail
    {
        get => _selectedWorkdayPeriodDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkdayPeriodDetail, value);
    }

    // add a SelectedWorkdayDetail property of type ShopWorkdayDetail
    private ShopWorkdayDetail? _selectedWorkdayDetail;

    public ShopWorkdayDetail? SelectedWorkdayDetail
    {
        get => _selectedWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkdayDetail, value);
    }

    // add a SearchCommand property of type ReactiveCommand<Unit, Unit>
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a IsBusy property of type bool
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // add a Constructor
    public ShopWorkdayPeriodDetailListViewModel()
    {
        // initialize the ShopWorkdayPeriodDetailList
        ShopWorkdayPeriodDetailList = new ObservableCollection<ShopWorkdayPeriodDetail>();

        // Create an observable that evaluates whether SearchCommand can execute
        var canExecuteSearch = this.WhenAnyValue(x => x.SelectedWorkdayDetail)
            .Select(x => x != null);

        // add a SearchCommand
        SearchCommand = ReactiveCommand.CreateFromObservable(
            execute: () => Observable.FromAsync(DoSearch),
            canExecute: canExecuteSearch);

        this.WhenActivated((disposables) =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name}: Activated");

            // handle the exception when the SearchCommand is executed
            SearchCommand.ThrownExceptions.Subscribe(ex =>
                {
                    Console.WriteLine("Failed to search for shop workday period detail");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);

            // set the IsBusy property to true when the SearchCommand is executing
            // code here 
            SearchCommand.IsExecuting.Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;

                    // set the IsBusy property
                    IsBusy = isExecuting;

                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus only if it is not the initial execution
                    if (!isInitial)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = isExecuting
                                    ? ActionStatus.StatusEnum.Executing
                                    : ActionStatus.StatusEnum.Completed,
                                Message = isExecuting
                                    ? "Searching for shop workday period detail list..."
                                    : "Shop workday period detail list search completed"
                            }));
                    }
                })
                .DisposeWith(disposables);

            // Listen to the ShopWorkdayDetailEvent
            MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
                .Subscribe(shopWorkdayDetailEvent =>
                {
                    SelectedWorkdayDetail = shopWorkdayDetailEvent.ShopWorkdayDetailMessage;
                })
                .DisposeWith(disposables);

            // when the SelectedWorkdayDetail property changes, call the DoSearch method
            // stop any previous search command if it is executing
            this.WhenAnyValue(x => x.SelectedWorkdayDetail)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Select(_ =>
                {
                    // clear the ShopWorkdayPeriodDetailList with RxApp.MainThreadScheduler
                    RxApp.MainThreadScheduler.Schedule(() => ShopWorkdayPeriodDetailList.Clear());

                    // execute the SearchCommand if selectedWorkdayDetail is not null
                    if (SelectedWorkdayDetail != null)
                    {
                        return SearchCommand.Execute();
                    }

                    return Observable.Empty<Unit>();
                })
                .Switch()
                .Subscribe(_ => { Console.WriteLine($"{GetType().Name}: Latest search completed"); })
                .DisposeWith(disposables);

            // when the SelectedWorkdayPeriodDetail property changes, emit the ShopWorkdayPeriodDetailEvent using the ReactiveUI MessageBus
            this.WhenAnyValue(x => x.SelectedWorkdayPeriodDetail)
                .Subscribe(shopWorkdayPeriodDetail =>
                {
                    MessageBus.Current.SendMessage(new ShopWorkdayPeriodDetailEvent(shopWorkdayPeriodDetail));
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

    public async Task DoSearch()
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

            // get the currentLoginSettings from Locator
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();

            // get the HTTP client from Locator
            var httpClient = Locator.Current.GetService<HttpClient>();

            // if currentLoginSettings or httpClient is null, return
            if (currentLoginSettings == null || httpClient == null) return;

            // if the SelectedWorkdayDetail is null, return
            if (SelectedWorkdayDetail == null) return;

            // call the API to get the shop workday period details
            // Endpoint: /api/PosAdmin/shopworkdayperioddetaillist?accountid={accountId}&shopid={shopId}&workdaydetailid={workdayDetailId}
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/shopworkdayperioddetaillist?accountid={SelectedWorkdayDetail.AccountId}&shopid={SelectedWorkdayDetail.ShopId}&workdaydetailid={SelectedWorkdayDetail.WorkdayDetailId}");
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return;

            // get the shop workday period details from the response
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var resultShopWorkdayPeriodDetailList = JsonSerializer.Deserialize<List<ShopWorkdayPeriodDetail>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];

            // add the shop workday period details to the ShopWorkdayPeriodDetailList in UI thread
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ShopWorkdayPeriodDetailList.Clear();
                foreach (var shopWorkdayPeriodDetail in resultShopWorkdayPeriodDetailList)
                {
                    ShopWorkdayPeriodDetailList.Add(shopWorkdayPeriodDetail);
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
            Console.WriteLine("An error occurred while searching for shop workday period detail");
            Console.WriteLine(e);
            throw;
        }
    }
}