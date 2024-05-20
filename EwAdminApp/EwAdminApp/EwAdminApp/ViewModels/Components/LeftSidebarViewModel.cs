using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class LeftSidebarViewModel : ViewModelBase
{
    private ObservableCollection<ModuleItem> _moduleItemList = [];

    public ObservableCollection<ModuleItem> ModuleItemList
    {
        get => _moduleItemList;
        set => this.RaiseAndSetIfChanged(ref _moduleItemList, value);
    }

    private ModuleItem? _selectedModuleItem;

    public ModuleItem? SelectedModuleItem
    {
        get => _selectedModuleItem;
        set => this.RaiseAndSetIfChanged(ref _selectedModuleItem, value);
    }

    public LeftSidebarViewModel()
    {
        this.WhenActivated(disposables =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");

            // listen to the MessageBus.Current.Listen<LoginEvent>()
            // and update the IsAccessible property of the ModuleItem accordingly
            MessageBus.Current.Listen<LoginEvent>()
                .Subscribe(OnLoginEventReceived)
                .DisposeWith(disposables);

            // emit the ModuleItemEvent when the SelectedModuleItem is changed
            this.WhenAnyValue(x => x.SelectedModuleItem)
                .WhereNotNull()
                .Subscribe(moduleItem =>
                {
                    //  console log the selected module item
                    Console.WriteLine($"{GetType().Name}: Selected module item: {moduleItem.DisplayName}");

                    // emit the ModuleItemEvent
                    MessageBus.Current.SendMessage(new ModuleItemEvent(moduleItem));
                })
                .DisposeWith(disposables);

            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} deactivated"))
                .DisposeWith(disposables);
        });
    }

    private void OnLoginEventReceived(LoginEvent loginEvent)
    {
        ModuleItemList.Add(new ModuleItem
        {
            DisplayName = "Home", Module = UserModuleEnum.HomeModule, IconResourceKey = "IconHomeRegular",
            IsAccessible = false
        });
        ModuleItemList.Add(new ModuleItem
        {
            DisplayName = "Fix Data", Module = UserModuleEnum.FixModule, IconResourceKey = "IconFixDataRegular",
            IsAccessible = false
        });
        ModuleItemList.Add(new ModuleItem
        {
            DisplayName = "View Data", Module = UserModuleEnum.ViewDataModule, IconResourceKey = "IconViewDataRegular",
            IsAccessible = false
        });
        ModuleItemList.Add(new ModuleItem
        {
            DisplayName = "Toolbox", Module = UserModuleEnum.ToolBoxModule, IconResourceKey = "IconToolboxRegular",
            IsAccessible = false
        });

        // assign the first item of the ModuleItemList to the SelectedModuleItem
        SelectedModuleItem = ModuleItemList.FirstOrDefault();
    }
}