using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

public class ReportTurnoverHeaderListViewModel : ViewModelBase
{
    private Shop? _selectedShop;
    private ObservableCollection<ReportTurnoverHeader> _reportTurnoverHeaderList = [];
    private ReportTurnoverHeader? _selectedReportTurnoverHeader;

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }

    public ObservableCollection<ReportTurnoverHeader> ReportTurnoverHeaderList
    {
        get => _reportTurnoverHeaderList;
        set => this.RaiseAndSetIfChanged(ref _reportTurnoverHeaderList, value);
    }

    public ReportTurnoverHeader? SelectedReportTurnoverHeader
    {
        get => _selectedReportTurnoverHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedReportTurnoverHeader, value);
    }

    // add a string property SearchText
    private string? _searchText
        = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    // add a properties IsBusy
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    // Add a command for SearchTextBox_OnKeyDown
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    public ReportTurnoverHeaderListViewModel()
    {
        // create an observable that evaluates whether SearchCommand can execute
        var canExecuteSearch = this.WhenAnyValue(x => x.SelectedShop)
            .Select(x => x != null);

        // create a SearchCommand that calls the DoSearch method
        SearchCommand = ReactiveCommand.CreateFromTask(
            execute: DoSearch,
            canExecute: canExecuteSearch);

        this.WhenActivated(disposables =>
        {
            // log the activation
            Console.WriteLine($"{GetType().Name} activated");

            // subscribe to the SearchCommand's IsExecuting property
            // and assign the value to the IsBusy property
            SearchCommand.IsExecuting
                .Subscribe(isExecuting =>
                {
                    // set the IsBusy property to the value of IsExecuting
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
                                Message = "Searching for ReportTurnoverHeader list..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // subscribe to the SelectedReportTurnoverHeader property
            // emit the ReportTurnoverHeaderEvent using the ReactiveUI MessageBus when the SelectedReportTurnoverHeader property changes
            this.WhenAnyValue(x => x.SelectedReportTurnoverHeader)
                .Subscribe(selectedReportTurnoverHeader =>
                {
                    // emit the ReportTurnoverHeaderEvent using the ReactiveUI MessageBus
                    MessageBus.Current.SendMessage(new ReportTurnoverHeaderEvent(selectedReportTurnoverHeader));
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
                        ReportTurnoverHeaderList.Clear();

                        // clear the SelectedReportTurnoverHeader property
                        SelectedReportTurnoverHeader = null;
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
                            Message = "ReportTurnoverHeader list search completed."
                        }));
                })
                .DisposeWith(disposables);

            // log the deactivation
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} deactivated");

                    // cancel the CancellationTokenSource
                    _cancellationTokenSource.Cancel();
                })
                .DisposeWith(disposables);
        });
    }

    // Implement the async Search ReportTurnoverHeader method
    // This method will be called when the SearchCommand is executed
    // This method will search the ReportTurnoverHeader based on the SearchText and SelectedShop
    // The result will be assigned to the ReportTurnoverHeaderList property
    // API Endpoint to use: /api/PosAdmin/reportTurnoverHeaderList?accountid=11377&shopid=5311&workdayOpenDatetimeGte=2023-07-08&workdayOpenDatetimeLte=2023-12-31
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
            RxApp.MainThreadScheduler.Schedule(() => ReportTurnoverHeaderList.Clear());

            // Perform the search operation
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            // Extract the date range from the SearchText
            var workdayOpenDatetimeGte = "";
            var workdayOpenDatetimeLte = "";
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchTextArrl = SearchText.Split(':');
                if (searchTextArrl.Length == 1)
                {
                    // case of ":2024-12-31", user want date less than or equal to 2024-12-31
                    if (searchTextArrl[0].StartsWith(":"))
                    {
                        workdayOpenDatetimeLte = searchTextArrl[0].Substring(1);
                    }
                    // case of "2024-05-01:", user want date greater than or equal to 2024-05-01
                    else if (searchTextArrl[0].EndsWith(":"))
                    {
                        workdayOpenDatetimeGte = searchTextArrl[0];
                    }
                    // case of "2024-12-01", user want exact date 2024-12-01
                    else
                    {
                        workdayOpenDatetimeGte = searchTextArrl[0];
                        workdayOpenDatetimeLte = searchTextArrl[0];
                    }
                }
                // case of "1000:2000", user want amount between 1000 and 2000 (inclusive)
                else if (searchTextArrl.Length == 2)
                {
                    workdayOpenDatetimeGte = searchTextArrl[0];
                    workdayOpenDatetimeLte = searchTextArrl[1];
                }
            }

            // Perform the search operation
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/PosAdmin/reportTurnoverHeaderList?accountid={SelectedShop?.AccountId}&shopid={SelectedShop?.ShopId}&workdayOpenDatetimeGte={workdayOpenDatetimeGte}&workdayOpenDatetimeLte={workdayOpenDatetimeLte}");
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
            var resultShopWorkdayDetailList = JsonSerializer.Deserialize<List<ReportTurnoverHeader>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // Assign the result to the ShopWorkdayDetailList property
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ReportTurnoverHeaderList =
                    new ObservableCollection<ReportTurnoverHeader>(resultShopWorkdayDetailList);
            });
        }
        catch (OperationCanceledException)
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