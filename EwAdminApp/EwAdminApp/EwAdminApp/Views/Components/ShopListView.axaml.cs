using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.Views.Components;

public partial class ShopListView : UserControl
{
    // public static readonly StyledProperty<Shop?> ShopProperty = 
    //     AvaloniaProperty.Register<ShopListView, Shop?>(nameof(Shop));

    public ShopListView()
    {
        InitializeComponent();
        if (Design.IsDesignMode) return;
        
    }

    private void SearchShopTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ShopListViewModel vm)
            {
                // execute the SearchCommand when the Enter key is pressed
                vm?.SearchCommand?.Execute().Subscribe();
                
                // check if the sender is a TextBox
                if (sender is TextBox textBox)
                {
                    // refocus the search box after SearchCommand is executed
                    vm?.SearchCommand?.IsExecuting
                        .Where(isExecuting => !isExecuting)
                        .Subscribe(_ => (sender as TextBox)?.Focus());
                }
            }
        }
        
        // if this is a down arrow key, focus the data grid
        if (e.Key == Key.Down)
        {
            if (sender is TextBox)
            {
                ShopListDataGrid.Focus();
            }
        }
    }
}