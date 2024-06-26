using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.ViewModels.Components;
using ReactiveUI;

namespace EwAdminApp.ViewModels.FixPages;

public class FixItemCategoryViewModel : ViewModelBase
{
    private ViewModelBase? _brandSelectorPanel;
    private ViewModelBase? _itemCategorySelectorPanel;
    private ViewModelBase? _itemCategoryDetailPanel;
    private ViewModelBase? _itemCategoryDetailEditPanel;
    
    public FixItemCategoryViewModel()
    {
        var brandSelectorPanelViewModel = new WebAdminBrandListViewModel();
        BrandSelectorPanel = brandSelectorPanelViewModel;

        var itemCategorySelectorPanelViewModel = new ItemCategoryListViewModel();
        ItemCategorySelectorPanel = itemCategorySelectorPanelViewModel;

        var itemCategoryDetailPanelViewModel = new ItemCategoryDetailViewModel();
        ItemCategoryDetailPanel = itemCategoryDetailPanelViewModel;

        var itemCategoryDetailEditPanelViewModel = new ItemCategoryDetailEditViewModel();
        ItemCategoryDetailEditPanel = itemCategoryDetailEditPanelViewModel;

        this.WhenActivated((disposables) =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }
    
    public ViewModelBase? BrandSelectorPanel
    {
        get => _brandSelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _brandSelectorPanel, value);
    }
    
    public ViewModelBase? ItemCategorySelectorPanel
    {
        get => _itemCategorySelectorPanel;
        set => this.RaiseAndSetIfChanged(ref _itemCategorySelectorPanel, value);
    }
    
    public ViewModelBase? ItemCategoryDetailPanel
    {
        get => _itemCategoryDetailPanel;
        set => this.RaiseAndSetIfChanged(ref _itemCategoryDetailPanel, value);
    }
    
    public ViewModelBase? ItemCategoryDetailEditPanel
    {
        get => _itemCategoryDetailEditPanel;
        set => this.RaiseAndSetIfChanged(ref _itemCategoryDetailEditPanel, value);
    }
}