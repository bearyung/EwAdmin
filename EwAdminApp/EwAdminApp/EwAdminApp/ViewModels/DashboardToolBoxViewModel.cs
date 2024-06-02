using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class DashboardToolBoxViewModel : ViewModelBase
{
    public ObservableCollection<FunctionItem> AvailableUserFunctions { get; set; } =
    [
        /*new FunctionItem
            { DisplayName = "Release POS License Key", Function = UserFunctionEnum.ToolboxReleaseLicenseKey },
        new FunctionItem
            { DisplayName = "Suspend DB Data Sync", Function = UserFunctionEnum.ToolboxSuspendDataSync },
        new FunctionItem
            { DisplayName = "Reset Client Data Sync", Function = UserFunctionEnum.ToolboxResetClientDataSync },
        // new FunctionItem
        //     { DisplayName = "Convert Item Category Type", Function = UserFunctionEnum.FixWorkdayDetail },
        new FunctionItem
            { DisplayName = "Rollback Modifier Flow", Function = UserFunctionEnum.ToolboxRollbackModifierFlow },
        new FunctionItem
            { DisplayName = "Copy Menu to New Account", Function = UserFunctionEnum.ToolboxCopyMenuToNewAccount },
        new FunctionItem
            { DisplayName = "Trim Data Sync Tracking", Function = UserFunctionEnum.ToolboxTrimDataSyncTracking },
        new FunctionItem
            { DisplayName = "Suspend POS User Accounts", Function = UserFunctionEnum.ToolboxSuspendPosUserAccounts },*/
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
    
    public DashboardToolBoxViewModel()
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
            /*case UserFunctionEnum.ToolboxReleaseLicenseKey:
                return new ToolboxReleaseLicenseKeyViewModel();
            case UserFunctionEnum.ToolboxSuspendDataSync:
                return new ToolboxSuspendDataSyncViewModel();
            case UserFunctionEnum.ToolboxResetClientDataSync:
                return new ToolboxResetClientDataSyncViewModel();
            case UserFunctionEnum.ToolboxRollbackModifierFlow:
                return new ToolboxRollbackModifierFlowViewModel();
            case UserFunctionEnum.ToolboxCopyMenuToNewAccount:
                return new ToolboxCopyMenuToNewAccountViewModel();
            case UserFunctionEnum.ToolboxTrimDataSyncTracking:
                return new ToolboxTrimDataSyncTrackingViewModel();*/
            default:
                return null;
        }
    }
    
    private void SwitchViewModel(ViewModelBase? viewModel)
    {
        SelectedContentViewModel = viewModel;
    }
}