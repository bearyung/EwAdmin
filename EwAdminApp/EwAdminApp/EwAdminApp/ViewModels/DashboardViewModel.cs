using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Models;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    public ObservableCollection<FunctionItem> AvailableUserFunctions { get; set; } =
    [
        new FunctionItem
            { DisplayName = "Fix Workday Detail", Function = UserFunctionEnum.FixWorkdayDetail },
        new FunctionItem
            { DisplayName = "Fix Workday Period Detail", Function = UserFunctionEnum.FixWorkdayPeriodDetail },
        new FunctionItem
            { DisplayName = "Fix Tx Payment", Function = UserFunctionEnum.FixTxPayment }
    ];

    private FunctionItem? _selectedUserFunction;
    private ViewModelBase? _selectedContentViewModel;

    public FunctionItem? SelectedUserFunction
    {
        get => _selectedUserFunction;
        set => this.RaiseAndSetIfChanged(ref _selectedUserFunction, value);
    }

    public ViewModelBase? SelectedContentViewModel
    {
        get => _selectedContentViewModel;
        set => this.RaiseAndSetIfChanged(ref _selectedContentViewModel, value);
    }

    public DashboardViewModel()
    {
        this.WhenActivated((disposables) =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");
            
            // When the selected user function changes, switch the view model
            this.WhenAnyValue(x => x.SelectedUserFunction)
                .Select(x => x != null ? SelectViewModel(x.Function) : null)
                .Subscribe(SwitchViewModel)
                .DisposeWith(disposables);

            // when the ExecutingCommandsCount property of the SelectedContentViewModel changes,
            // update the view model's ExecutingCommandsCount property
            this.WhenAnyValue(x => x.SelectedContentViewModel!.ExecutingCommandsCount)
                .Subscribe(x =>
                {
                    // log the ExecutingCommandsCount property
                    Console.WriteLine($"{GetType().Name}: ExecutingCommandsCount: {x}");

                    // Update the ExecutingCommandsCount property
                    ExecutingCommandsCount = x;
                })
                .DisposeWith(disposables);
            
            // log the deactivation of the viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }

    private ViewModelBase? SelectViewModel(UserFunctionEnum function)
    {
        switch (function)
        {
            case UserFunctionEnum.FixWorkdayDetail:
                return new FixShopWorkdayDetailViewModel();
            case UserFunctionEnum.FixWorkdayPeriodDetail:
                return new FixShopWorkdayPeriodDetailViewModel();
            case UserFunctionEnum.FixTxPayment:
                return new FixTxPaymentViewModel();
            default:
                return null;
        }
    }

    private void SwitchViewModel(ViewModelBase? viewModel)
    {
        SelectedContentViewModel = viewModel;
    }
}