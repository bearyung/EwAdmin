using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Models;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<FunctionItem> AvailableUserFunctions { get; set; } =
    [
        new FunctionItem
        {
            DisplayName = "Check for Updates", Function = UserFunctionEnum.SettingsCheckForUpdates
        }
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
    
    public SettingsViewModel()
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
            
            // log the deactivation of viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} deactivated"))
                .DisposeWith(disposables);
        });
    }
    
    private ViewModelBase? SelectViewModel(UserFunctionEnum function)
    {
        switch (function)
        {
            case UserFunctionEnum.SettingsCheckForUpdates:
                return new SettingsCheckForUpdatesViewModel();
            default:
                return null;
        }
    }

    private void SwitchViewModel(ViewModelBase? viewModel)
    {
        SelectedContentViewModel = viewModel;
    }
}