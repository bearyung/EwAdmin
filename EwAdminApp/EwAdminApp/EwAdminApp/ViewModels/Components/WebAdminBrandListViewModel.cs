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
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels.Components;

public class WebAdminBrandListViewModel : ViewModelBase
{
    private ObservableCollection<BrandMaster> _brandList = [];
    private BrandMaster? _selectedBrand;
    private bool _isBusy;
    private string? _searchTextCompanyId;
    private string? _searchTextBrandId;

    public ObservableCollection<BrandMaster> BrandList
    {
        get => _brandList;
        set => this.RaiseAndSetIfChanged(ref _brandList, value);
    }

    public BrandMaster? SelectedBrand
    {
        get => _selectedBrand;
        set => this.RaiseAndSetIfChanged(ref _selectedBrand, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    public string? SearchTextCompanyId
    {
        get => _searchTextCompanyId;
        set => this.RaiseAndSetIfChanged(ref _searchTextCompanyId, value);
    }

    public string? SearchTextBrandId
    {
        get => _searchTextBrandId;
        set => this.RaiseAndSetIfChanged(ref _searchTextBrandId, value);
    }

    public ReactiveCommand<Unit, Unit>? SearchCommand { get; }

    // add a cancellationTokenSource property
    private CancellationTokenSource _cancellationTokenSource = new();

    public WebAdminBrandListViewModel()
    {
        // initialize the SearchCommand property with a new ReactiveCommand
        SearchCommand = ReactiveCommand.CreateFromTask(DoSearch);

        this.WhenActivated(disposables =>
        {
            // console log the activation
            Console.WriteLine($"{GetType().Name} activated");

            // set the isBusy property to true when the search command is executing
            SearchCommand.IsExecuting.Subscribe(isExecuting =>
                {
                    var isInitial = ExecutingCommandsCount == 0 && !isExecuting;

                    IsBusy = isExecuting;

                    // increment or decrement the ExecutingCommandsCount property
                    ExecutingCommandsCount += isExecuting ? 1 : (ExecutingCommandsCount > 0 ? -1 : 0);

                    // emit the ActionStatusMessageEvent using the ReactiveUI MessageBus
                    if (!isInitial && isExecuting)
                    {
                        MessageBus.Current.SendMessage(new ActionStatusMessageEvent(
                            new ActionStatus
                            {
                                ActionStatusEnum = ActionStatus.StatusEnum.Executing,
                                Message = "Searching for brands..."
                            }));
                    }
                })
                .DisposeWith(disposables);

            // Subscribe to the ThrownExceptions property of the SearchCommand property
            SearchCommand.ThrownExceptions.Subscribe(ex =>
            {
                // log the exception
                Console.WriteLine($"{GetType().Name}: Failed to search for shops");
                Console.WriteLine(ex.Message);
            });

            // Subscribe to the ExecutingCommandsCount property
            this.WhenAnyValue(x => x.ExecutingCommandsCount)
                .Subscribe(count =>
                {
                    // log the ExecutingCommandsCount property value
                    Console.WriteLine($"{GetType().Name}: ExecutingCommandsCount: {count}");
                })
                .DisposeWith(disposables);
            
            // when the SelectedBrand property changes, emit the WebAdminBrandEvent using the ReactiveUI MessageBus
            this.WhenAnyValue(x => x.SelectedBrand)
                .Subscribe(brand =>
                {
                    // log the selected brand
                    Console.WriteLine($"{GetType().Name}: Selected brand: {brand?.BrandId}");
                    
                    // emit the WebAdminBrandEvent
                    MessageBus.Current.SendMessage(new WebAdminBrandEvent(brand));
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
                            Message = "Brand search completed"
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

    private async Task DoSearch()
    {
        try
        {
            // cancel the previous search if it is still running
            await _cancellationTokenSource.CancelAsync();

            // create a new cancellation token source
            _cancellationTokenSource = new CancellationTokenSource();

            // get the token from the cancellation token source
            var cancellationToken = _cancellationTokenSource.Token;

            // throw an OperationCanceledException if the token is cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // clear the BrandList property
            RxApp.MainThreadScheduler.Schedule(() => BrandList.Clear());

            // perform the search operation
            // use the SearchTextCompanyId and SearchTextBrandId properties to filter the search
            // use HTTPClient registered in the Splat container to make the API request
            // update the BrandList property with the result of the search
            var currentLoginSettings = Locator.Current.GetService<LoginSettings>();
            if (currentLoginSettings == null) return;

            var httpClient = Locator.Current.GetService<HttpClient>();
            if (httpClient == null) return;

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/WebAdmin/brandList?" +
                $"companyId={SearchTextCompanyId}&brandId={SearchTextBrandId}");
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

            // parse the response content to a list of BrandMaster objects
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var resultBrandList = JsonSerializer.Deserialize<List<BrandMaster>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            // update the BrandList property with the result of the search
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                foreach (var brand in resultBrandList)
                {
                    BrandList?.Add(brand);

                    // set the first brand in the list as the selected brand
                    SelectedBrand = BrandList?.FirstOrDefault();
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
            Console.WriteLine(e);
            throw;
        }
    }
}