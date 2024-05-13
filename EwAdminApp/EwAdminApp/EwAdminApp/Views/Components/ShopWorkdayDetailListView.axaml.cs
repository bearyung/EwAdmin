using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayDetailListView : UserControl
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
            }
        }
    }
}