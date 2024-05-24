using System;
using System.Reactive.Disposables;
using EwAdmin.Common.Models.Pos;
using EwAdminApp.Events;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class ItemCategoryDetailViewModel : ViewModelBase
{
    private ItemCategory? _selectedItemCategory;

    public ItemCategory? SelectedItemCategory
    {
        get => _selectedItemCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedItemCategory, value);
    }
    
    // add a constructor
    // code here 
    public ItemCategoryDetailViewModel()
    {
        this.WhenActivated(disposables =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            
            // when the SelectedItemCategory property changes, console log the item category
            this.WhenAnyValue(x => x.SelectedItemCategory)
                .Subscribe(itemCategory =>
                {
                    if (itemCategory != null)
                    {
                        Console.WriteLine($"ItemCategory: {itemCategory.CategoryId}");
                    }
                })
                .DisposeWith(disposables);
            
            // subscribe to the ItemCategoryEvent
            MessageBus.Current.Listen<ItemCategoryEvent>()
                .Subscribe(itemCategoryEvent =>
                {
                    SelectedItemCategory = itemCategoryEvent.ItemCategoryMessage;
                })
                .DisposeWith(disposables);
            
            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposables);
        });
    }
}