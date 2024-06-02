using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Models;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class DashboardViewDataViewModel : ViewModelBase
{
    public ObservableCollection<FunctionItem> AvailableUserFunctions { get; set; } =
    [
        /*new FunctionItem
            { DisplayName = "View Day-end History", Function = UserFunctionEnum.ViewDayEndHistory },
        new FunctionItem
            { DisplayName = "View TxPayment History", Function = UserFunctionEnum.ViewTxPaymentHistory },
        new FunctionItem
            { DisplayName = "View Clock-in/out History", Function = UserFunctionEnum.ViewClockInOutHistory },
        new FunctionItem
            { DisplayName = "View Account Information", Function = UserFunctionEnum.ViewAccountInformation },
        new FunctionItem
            { DisplayName = "View EwAPI Log", Function = UserFunctionEnum.ViewEwApiLog },*/
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
    
public DashboardViewDataViewModel()
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
        });
    }

    private ViewModelBase? SelectViewModel(UserFunctionEnum function)
    {
        switch (function)
        {
            default:
                return null;
        }
    }
    
    private void SwitchViewModel(ViewModelBase? viewModel)
    {
        SelectedContentViewModel = viewModel;
    }
}