using System;
using System.Reactive.Disposables;
using System.Threading;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    // Add common properties and methods here
    // add a property to store the number of current viewmodel's execting commands
    private int _executingCommandsCount;
    
    // add a property for Activator
    public ViewModelActivator Activator { get; } = new();

    public int ExecutingCommandsCount
    {
        get => _executingCommandsCount;
        set => this.RaiseAndSetIfChanged(ref _executingCommandsCount, value);
    }
}