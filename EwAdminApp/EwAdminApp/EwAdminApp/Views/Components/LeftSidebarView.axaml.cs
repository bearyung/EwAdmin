using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.Components;

namespace EwAdminApp.Views.Components;

public partial class LeftSidebarView : ReactiveUserControl<LeftSidebarViewModel>
{
    public LeftSidebarView()
    {
        InitializeComponent();
    }
}