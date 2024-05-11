using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class TxSalesHeaderListView : UserControl
{
    public TxSalesHeaderListView()
    {
        InitializeComponent();
    }

    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Check if DataContext is TxSalesHeaderListViewModel
            // If true, execute the SearchCommand
            if (DataContext is TxSalesHeaderListViewModel vm)
            {
                vm.SearchCommand.Execute().Subscribe();
            }
        }
    }
}