using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ShopWorkdayListView : UserControl
{
    public ShopWorkdayListView()
    {
        InitializeComponent();
    }


    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ShopWorkdayListViewModel vm)
            {
                vm.SearchCommand.Execute().Subscribe();
            }
        }
    }
}