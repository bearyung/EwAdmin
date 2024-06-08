using System;
using System.Reactive.Disposables;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.ViewPages;

public class ViewDayendDetailViewModel : ViewModelBase
{
    private ViewModelBase? _shopSelectorPanel;
    private ViewModelBase? _reportTurnoverHeaderSelectorPanel;
    
    public ViewDayendDetailViewModel()
    {
        var shopSelectorPanelViewModel = new ShopListViewModel();
        ShopSelectorPanel = shopSelectorPanelViewModel;

        var reportTurnoverHeaderSelectorPanelViewModel = new ReportTurnoverHeaderListViewModel();
        ReportTurnoverHeaderSelectorPanel = reportTurnoverHeaderSelectorPanelViewModel;

        this.WhenActivated((disposables) =>
        {
            Console.WriteLine($"{GetType().Name} activated");
            
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }
    
    public ViewModelBase? ShopSelectorPanel
    {
        get => _shopSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _shopSelectorPanel, value);
    }
    
    public ViewModelBase? ReportTurnoverHeaderSelectorPanel
    {
        get => _reportTurnoverHeaderSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _reportTurnoverHeaderSelectorPanel, value);
    }
}