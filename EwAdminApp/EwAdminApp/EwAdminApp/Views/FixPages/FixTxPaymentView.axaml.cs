using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;
using EwAdminApp.ViewModels.FixPages;

namespace EwAdminApp.Views.FixPages;

public partial class FixTxPaymentView : ReactiveUserControl<FixTxPaymentViewModel>
{
    public FixTxPaymentView()
    {
        InitializeComponent();

        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        //if (Design.IsDesignMode) return;

        // add the data context
        //DataContext = new FixTxPaymentViewModel();
    }
}