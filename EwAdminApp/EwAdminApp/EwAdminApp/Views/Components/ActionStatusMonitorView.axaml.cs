using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class ActionStatusMonitorView : ReactiveUserControl<ActionStatusMonitorViewModel>
{
    public ActionStatusMonitorView()
    {
        InitializeComponent();
    }
}