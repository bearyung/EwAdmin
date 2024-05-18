using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
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
    // code here
    private ObservableCollection<ShopWorkdayPeriodDetail> _shopWorkdayPeriodDetailList = [];

    public ObservableCollection<ShopWorkdayPeriodDetail> ShopWorkdayPeriodDetailList
    {
        get => _shopWorkdayPeriodDetailList;
        set => this.RaiseAndSetIfChanged(ref _shopWorkdayPeriodDetailList, value);
    }

    // add a SelectedWorkdayPeriodDetail property of type ShopWorkdayPeriodDetail
    // code here
    private ShopWorkdayPeriodDetail? _selectedWorkdayPeriodDetail;

    public ShopWorkdayPeriodDetail? SelectedWorkdayPeriodDetail
    {
        get => _selectedWorkdayPeriodDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkdayPeriodDetail, value);
    }

    // add a SelectedWorkdayDetail property of type ShopWorkdayDetail
    // code here
    private ShopWorkdayDetail? _selectedWorkdayDetail;

    public ShopWorkdayDetail? SelectedWorkdayDetail
    {
        get => _selectedWorkdayDetail;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkdayDetail, value);
    }

    // add a SearchCommand property of type ReactiveCommand<Unit, Unit>
    // code here
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    // add a IsBusy property of type bool
    // code here
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a Constructor
    // code here
    public ShopWorkdayPeriodDetailListViewModel()
    {
        // initialize the ShopWorkdayPeriodDetailList
        // code here
        ShopWorkdayPeriodDetailList = new ObservableCollection<ShopWorkdayPeriodDetail>();

        // Create an observable that evaluates whether SearchCommand can execute
        // code here
        var canExecuteSearch = this.WhenAnyValue(x => x.SelectedWorkdayDetail)
            .Select(x => x != null);

        // add a SearchCommand
        // code here
        SearchCommand = ReactiveCommand.CreateFromObservable(
            execute: () => Observable.FromAsync(DoSearch),
            canExecute: canExecuteSearch);

        this.WhenActivated((disposables) =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name}: Activated");
            
            // handle the exception when the SearchCommand is executed
            // code here
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
            // code here
            MessageBus.Current.Listen<ShopWorkdayDetailEvent>()
                .Subscribe(shopWorkdayDetailEvent =>
                {
                    SelectedWorkdayDetail = shopWorkdayDetailEvent.ShopWorkdayDetailMessage;
                })
                .DisposeWith(disposables);

            // when the SelectedWorkdayDetail property changes, call the DoSearch method
            // stop any previous search command if it is executing
            // code here
            this.WhenAnyValue(x => x.SelectedWorkdayDetail)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Select(_ =>
                {
                    // clear the ShopWorkdayPeriodDetailList with RxApp.MainThreadScheduler
                    // code here
                    RxApp.MainThreadScheduler.Schedule(() => ShopWorkdayPeriodDetailList.Clear());
                    
                    // execute the SearchCommand if selectedWorkdayDetail is not null
                    // code here
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
            // code here
            this.WhenAnyValue(x => x.SelectedWorkdayPeriodDetail)
                .Subscribe(shopWorkdayPeriodDetail =>
                {
                    MessageBus.Current.SendMessage(new ShopWorkdayPeriodDetailEvent(shopWorkdayPeriodDetail));
                })
                .DisposeWith(disposables);
            
            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    public async Task DoSearch()
    {
        try
        {
            // get the currentLoginSettings from Locator
            // code here
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();

            // get the HTTP client from Locator
            // code here
            var httpClient = Locator.Current.GetService<HttpClient>();

            // if currentLoginSettings or httpClient is null, return
            // code here
            if (currentLoginSettings == null || httpClient == null) return;

            // if the SelectedWorkdayDetail is null, return
            // code here
            if (SelectedWorkdayDetail == null) return;

            // call the API to get the shop workday period details
            // Endpoint: /api/PosAdmin/shopworkdayperioddetaillist?accountid={accountId}&shopid={shopId}&workdaydetailid={workdayDetailId}
            // code here
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/shopworkdayperioddetaillist?accountid={SelectedWorkdayDetail.AccountId}&shopid={SelectedWorkdayDetail.ShopId}&workdaydetailid={SelectedWorkdayDetail.WorkdayDetailId}");
            request.Headers.Add("Authorization", $"Bearer {currentLoginSettings.ApiKey}");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return;

            // get the shop workday period details from the response
            // code here
            var content = await response.Content.ReadAsStringAsync();
            var resultShopWorkdayPeriodDetailList = JsonSerializer.Deserialize<List<ShopWorkdayPeriodDetail>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];

            // add the shop workday period details to the ShopWorkdayPeriodDetailList in UI thread
            // code here
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ShopWorkdayPeriodDetailList.Clear();
                foreach (var shopWorkdayPeriodDetail in resultShopWorkdayPeriodDetailList)
                {
                    ShopWorkdayPeriodDetailList.Add(shopWorkdayPeriodDetail);
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while searching for shop workday period detail");
            Console.WriteLine(e);
            throw;
        }
    }
}