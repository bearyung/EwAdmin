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
    private ObservableCollection<ModuleItem> _functionalModuleItemList = [];

    public ObservableCollection<ModuleItem> FunctionalModuleItemList
    {
        get => _functionalModuleItemList;
        set => this.RaiseAndSetIfChanged(ref _functionalModuleItemList, value);
    }

    private ObservableCollection<ModuleItem> _generalModuleItemList = [];

    public ObservableCollection<ModuleItem> GeneralModuleItemList
    {
        get => _generalModuleItemList;
        set => this.RaiseAndSetIfChanged(ref _generalModuleItemList, value);
    }

    private ModuleItem? _selectedFunctionalModuleItem;
    private ModuleItem? _selectedGeneralModuleItem;

    public ModuleItem? SelectedFunctionalModuleItem
    {
        get => _selectedFunctionalModuleItem;
        set => this.RaiseAndSetIfChanged(ref _selectedFunctionalModuleItem, value);
    }
    
    public ModuleItem? SelectedGeneralModuleItem
    {
        get => _selectedGeneralModuleItem;
        set => this.RaiseAndSetIfChanged(ref _selectedGeneralModuleItem, value);
    }

    public LeftSidebarViewModel()
    {
        this.WhenActivated(disposables =>
        {
            // console log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");
            
            // initialize the FunctionalModuleItemList
            InitGeneralModuleItemList();
            
            // listen to SelectedFunctionalModuleItem and SelectedGeneralModuleItem
            // set either of them to null when the other is set
            this.WhenAnyValue(x => x.SelectedFunctionalModuleItem)
                .WhereNotNull()
                .Subscribe(_ => SelectedGeneralModuleItem = null)
                .DisposeWith(disposables);
            
            this.WhenAnyValue(x => x.SelectedGeneralModuleItem)
                .WhereNotNull()
                .Subscribe(_ => SelectedFunctionalModuleItem = null)
                .DisposeWith(disposables);

            // listen to the MessageBus.Current.Listen<LoginEvent>()
            // and update the IsAccessible property of the ModuleItem accordingly
            MessageBus.Current.Listen<LoginEvent>()
                .Subscribe(OnLoginEventReceived)
                .DisposeWith(disposables);

            // emit the ModuleItemEvent when the SelectedFunctionalModuleItem is changed
            this.WhenAnyValue(x => x.SelectedFunctionalModuleItem)
                .WhereNotNull()
                .Subscribe(moduleItem =>
                {
                    //  console log the selected module item
                    Console.WriteLine($"{GetType().Name}: Selected functional module item: {moduleItem.DisplayName}");

                    // emit the ModuleItemEvent
                    MessageBus.Current.SendMessage(new ModuleItemEvent(moduleItem));
                })
                .DisposeWith(disposables);
            
            // emit the ModuleItemEvent when the SelectedGeneralModuleItem is changed
            this.WhenAnyValue(x => x.SelectedGeneralModuleItem)
                .WhereNotNull()
                .Subscribe(moduleItem =>
                {
                    //  console log the selected module item
                    Console.WriteLine($"{GetType().Name}: Selected general module item: {moduleItem.DisplayName}");

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
        // clear the functional module item list if loginEvent.LoginSettings is null
        if (loginEvent.LoginSettings == null)
        {
            FunctionalModuleItemList.Clear();
            SelectedFunctionalModuleItem = null;
            SelectedGeneralModuleItem = null;
        }
        else
        {
            // initialize the FunctionalModuleItemList if loginEvent.LoginSettings is not null
            InitFunctionalModuleItemList();
        }
    }

    private void InitFunctionalModuleItemList()
    {
        // add the ModuleItem to the ModuleItemList
        // do this in one batch, not one by one
        // code here

        FunctionalModuleItemList = new ObservableCollection<ModuleItem>()
        {
            new()
            {
                DisplayName = "Home", Module = UserModuleEnum.HomeModule,
                IconResourceKey = "IconHomeRegular",
                IsAccessible = false
            },
            new()
            {
                DisplayName = "Fix Data", Module = UserModuleEnum.FixModule,
                IconResourceKey = "IconWrenchRegular",
                IsAccessible = false
            },
            new()
            {
                DisplayName = "View Data", Module = UserModuleEnum.ViewDataModule,
                IconResourceKey = "IconDocumentSearchRegular",
                IsAccessible = false
            },
            new()
            {
                DisplayName = "Toolbox", Module = UserModuleEnum.ToolBoxModule,
                IconResourceKey = "IconToolboxRegular",
                IsAccessible = false
            }
        };

        SelectedFunctionalModuleItem = FunctionalModuleItemList.FirstOrDefault();
    }

    private void InitGeneralModuleItemList()
    {
        // add the ModuleItem to the ModuleItemList
        // do this in one batch, not one by one
        // code here

        GeneralModuleItemList = new ObservableCollection<ModuleItem>()
        {
            new()
            {
                DisplayName = "Settings", Module = UserModuleEnum.SettingsModule,
                IconResourceKey = "IconSettingsRegular",
                IsAccessible = false
            },
            new()
            {
                DisplayName = "Help", Module = UserModuleEnum.HelpModule,
                IconResourceKey = "IconQuestionCircleRegular",
                IsAccessible = false
            }
        };
    }
}