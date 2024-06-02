using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class TableMasterListView : ReactiveUserControl<TableMasterListViewModel>
{
    public TableMasterListView()
    {
        InitializeComponent();
    }

    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}