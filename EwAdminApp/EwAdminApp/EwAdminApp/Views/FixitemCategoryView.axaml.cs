using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;

namespace EwAdminApp.Views;

public partial class FixItemCategoryView : ReactiveUserControl<FixItemCategoryViewModel>
{
    public FixItemCategoryView()
    {
        InitializeComponent();
    }
}