using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ReportTurnoverHeaderListView : ReactiveUserControl<ReportTurnoverHeaderListViewModel>
{
    public ReportTurnoverHeaderListView()
    {
        InitializeComponent();
    }

    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is not ReportTurnoverHeaderListViewModel vm) return;
            vm.SearchCommand.Execute().Subscribe();
                
            // check if the sender is a TextBox
            if (sender is TextBox)
            {
                // refocus the search box after SearchCommand is executed
                vm.SearchCommand.IsExecuting
                    .Where(isExecuting => !isExecuting)
                    .Subscribe(_ => (sender as TextBox)?.Focus());
            }
        }
    }
}