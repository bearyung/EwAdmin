using System;
using System.Reactive;
using System.Reactive.Disposables;
using DialogHostAvalonia;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Dialogs;

public class TableMasterListDialogViewModel : ViewModelBase
{
    private ViewModelBase? _tableSelectorPanel;
    private TableMaster? _selectedTableMaster;
    private Shop? _selectedShop;
    private bool _isCancelled;

    public ViewModelBase? TableSelectorPanel
    {
        get => _tableSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _tableSelectorPanel, value);
    }

    public TableMaster? SelectedTableMaster
    {
        get => _selectedTableMaster;
        set => this.RaiseAndSetIfChanged(ref _selectedTableMaster, value);
    }

    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }
    
    public bool IsCancelled
    {
        get => _isCancelled;
        set => this.RaiseAndSetIfChanged(ref _isCancelled, value);
    }
    
    // add a command for DialogHost closing event
    public ReactiveCommand<bool, Unit> CloseDialogCommand { get; set; }

    public TableMasterListDialogViewModel(Shop? selectedShop)
    {
        SelectedShop = selectedShop;

        ViewModelInitComplete();
    }

    public TableMasterListDialogViewModel()
    {
        ViewModelInitComplete();
    }

    private void ViewModelInitComplete()
    {
        TableSelectorPanel = new TableMasterListViewModel();

        // set the SelectedShop property of TableMasterListViewModel
        if (TableSelectorPanel is TableMasterListViewModel tableMasterListViewModel)
        {
            tableMasterListViewModel.SelectedShop = SelectedShop;
        }
        
        // initialize the CloseDialogCommand property with a new ReactiveCommand
        CloseDialogCommand = ReactiveCommand.Create<bool>(isCancelled =>
        {
            IsCancelled = isCancelled;
            DialogHost.Close(null);
        });

        this.WhenActivated(disposables =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");

            // log the deactivation of the viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);

            // listen to TableSelectorPanel's SelectedTableMaster property if this is a TableMasterListViewModel
            // and log the value when it changes
            if (TableSelectorPanel is TableMasterListViewModel vm)
            {
                vm
                    .WhenAnyValue(x => x.SelectedTableMaster)
                    .Subscribe(selectedTableMaster =>
                    {
                        Console.WriteLine(
                            $"Selected TableMaster: {selectedTableMaster?.TableId}: {selectedTableMaster?.TableCode}");

                        SelectedTableMaster = selectedTableMaster;
                    })
                    .DisposeWith(disposables);
            }
        });
    }
}