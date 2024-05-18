using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayDetailListView : ReactiveUserControl<ShopWorkdayDetailListViewModel>
{
    public ShopWorkdayDetailListView()
    {
        InitializeComponent();
    }


    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ShopWorkdayDetailListViewModel vm)
            {
                vm.SearchCommand.Execute().Subscribe();
                
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
    }
}