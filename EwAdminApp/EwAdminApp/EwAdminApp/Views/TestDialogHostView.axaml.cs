using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;

namespace EwAdminApp.Views;

public partial class TestDialogHostView : UserControl
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