using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Models;
using EwAdminApp.ViewModels.FixPages;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class DashboardFixViewModel : ViewModelBase
{
    public ObservableCollection<FunctionItem> AvailableUserFunctions { get; set; } =
    [
        new FunctionItem
            { DisplayName = "Fix Workday Detail", Function = UserFunctionEnum.FixWorkdayDetail },
        new FunctionItem
            { DisplayName = "Fix Workday Period Detail", Function = UserFunctionEnum.FixWorkdayPeriodDetail },
        new FunctionItem
            { DisplayName = "Fix Tx Payment", Function = UserFunctionEnum.FixTxPayment },
        new FunctionItem
            { DisplayName = "Fix Item Category", Function = UserFunctionEnum.FixItemCategory },
        // new FunctionItem
        //     { DisplayName = "Fix Incorrect Day-end", Function = UserFunctionEnum.FixIncorrectDayEnd},
        // new FunctionItem
        //     { DisplayName = "Fix Table Master", Function = UserFunctionEnum.FixTableMaster },
        new FunctionItem
            { DisplayName = "Remap Table to Tx", Function = UserFunctionEnum.FixTxTableRemap },
        // new FunctionItem
        //     { DisplayName = "Remap Cash Drawer to Tx", Function = UserFunctionEnum.FixTxCashDrawerRemap },
        new FunctionItem
            { DisplayName = "Adjust Tx Cus Count", Function = UserFunctionEnum.FixTxCustomerCount },
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

    public DashboardFixViewModel()
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
            case UserFunctionEnum.FixItemCategory:
                return new FixItemCategoryViewModel();
            case UserFunctionEnum.FixTxCustomerCount:
                return new FixTxSalesHeaderCusCountViewModel();
            case UserFunctionEnum.FixTxTableRemap:
                return new FixTxSalesHeaderTableMappingViewModel();
            default:
                return null;
        }
    }

    private void SwitchViewModel(ViewModelBase? viewModel)
    {
        SelectedContentViewModel = viewModel;
    }
}