using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DialogHostAvalonia;
using EwAdminApp.ViewModels.Dialogs;

namespace EwAdminApp.Views.Dialogs;

public partial class TableMasterListDialogView : ReactiveUserControl<TableMasterListDialogViewModel>
{
    public TableMasterListDialogView()
    {
        InitializeComponent();
    }
}