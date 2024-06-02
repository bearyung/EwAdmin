using System;
using System.Reactive;
using DialogHostAvalonia;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Views;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class TxSalesHeaderDetailEditTableViewModel : ViewModelBase
{
    // properties for the view
    // selectedShop, selectedTableMaster, selectedTxSalesHeader
    // code here
    
    private Shop? _selectedShop;
    private TableMaster? _selectedTableMaster;
    private TxSalesHeader? _selectedTxSalesHeader;
    
    public Shop? SelectedShop
    {
        get => _selectedShop;
        set => this.RaiseAndSetIfChanged(ref _selectedShop, value);
    }
    
    public TableMaster? SelectedTableMaster
    {
        get => _selectedTableMaster;
        set => this.RaiseAndSetIfChanged(ref _selectedTableMaster, value);
    }
    
    public TxSalesHeader? SelectedTxSalesHeader
    {
        get => _selectedTxSalesHeader;
        set => this.RaiseAndSetIfChanged(ref _selectedTxSalesHeader, value);
    }
    
    // add the ReactiveCommand property
    // SelectTableCommand
    // code here
    public ReactiveCommand<Unit, Unit>? SelectTableCommand { get; }
    
    // add the constructor
    // code here
    public TxSalesHeaderDetailEditTableViewModel()
    {
        // initialize the SelectTableCommand property with a new ReactiveCommand
        // code here
        SelectTableCommand = ReactiveCommand.Create(() =>
        {
            // popup a viewmodel to select a table using DialogHost.Avalonia
            // code here
            DialogHost.Show(new TestDialogHostView());
        });
    }
}