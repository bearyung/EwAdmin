using System;
using System.Collections.ObjectModel;
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
        // When the selected user function changes, switch the view model
        this.WhenAnyValue(x => x.SelectedUserFunction)
            .Select(x => x != null ? SelectViewModel(x.Function) : null)
            .Subscribe(SwitchViewModel);
    }
    
    private ViewModelBase? SelectViewModel(UserFunctionEnum function)
    {
        switch (function)
        {
            case UserFunctionEnum.FixWorkdayDetail:
                //return new FixTxPaymentViewModel();
            case UserFunctionEnum.FixWorkdayPeriodDetail:
                //return new FixWorkdayPeriodDetailViewModel();
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