using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;
using EwAdminApp.ViewModels.FixPages;

namespace EwAdminApp.Views.FixPages;

public partial class FixItemCategoryView : ReactiveUserControl<FixItemCategoryViewModel>
{
    public FixItemCategoryView()
    {
        InitializeComponent();
    }
}