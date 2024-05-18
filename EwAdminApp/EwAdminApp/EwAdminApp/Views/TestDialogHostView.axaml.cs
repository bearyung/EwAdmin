using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DialogHostAvalonia;
using EwAdminApp.ViewModels;

namespace EwAdminApp.Views;

public partial class TestDialogHostView : ReactiveUserControl<TestDialogHostViewModel>
{
    public TestDialogHostView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogHost.Show(new TestDialogHostView());
    }
}