using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class WebAdminBrandListView : ReactiveUserControl<WebAdminBrandListViewModel>
{
    public WebAdminBrandListView()
    {
        InitializeComponent();
    }

    private void SearchBrandTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is WebAdminBrandListViewModel vm)
            {
                // execute the SearchCommand when the Enter key is pressed
                vm.SearchCommand?.Execute().Subscribe();

                // check if the sender is a TextBox
                if (sender is TextBox)
                {
                    // refocus the search box after SearchCommand is executed
                    vm.SearchCommand?.IsExecuting
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
                BrandListDataGrid.Focus();
            }
        }
    }

}