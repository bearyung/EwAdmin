using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels.FixPages;

namespace EwAdminApp.Views.FixPages;

public partial class FixTxSalesHeaderTableMappingView : ReactiveUserControl<FixTxSalesHeaderTableMappingViewModel>
{
    public FixTxSalesHeaderTableMappingView()
    {
        InitializeComponent();
    }
}