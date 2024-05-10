using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EwAdminApp.ViewModels;

namespace EwAdminApp.Views;

public partial class FixTxPaymentView : UserControl
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