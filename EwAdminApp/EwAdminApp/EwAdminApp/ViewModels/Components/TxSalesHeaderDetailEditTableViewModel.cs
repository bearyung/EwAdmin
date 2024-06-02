using System;
using System.Reactive;
using System.Reactive.Disposables;
using DialogHostAvalonia;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
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
            // DialogHost.Show(new TableMasterListViewModel());
            DialogHost.Show(new TestDialogHostViewModel());
        });
        
        this.WhenActivated(disposables =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // listen to the Message Bus for the TxSalesHeaderEvent
            // when the event is received, update the SelectedTxSalesHeader property
            // code here
            MessageBus.Current.Listen<TxSalesHeaderEvent>()
                .Subscribe(x =>
                {
                    SelectedTxSalesHeader = x.TxSalesHeaderMessage;
                })
                .DisposeWith(disposables);
            
            // listen to the message bus for the ShopEvent
            // when the event is received, update the SelectedShop property
            // code here
            MessageBus.Current.Listen<ShopEvent>()
                .Subscribe(x =>
                {
                    SelectedShop = x.ShopMessage;
                })
                .DisposeWith(disposables);
            
            // log when the viewmodel is deactivated
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");
                })
                .DisposeWith(disposables);
        });
    }
}