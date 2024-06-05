using System;
using System.Reactive.Disposables;
using System.Threading;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    // add a property for Activator
    public ViewModelActivator Activator { get; } = new();

    private bool _isStandaloneMode;
    
    public bool IsStandaloneMode
    {
        get => _isStandaloneMode;
        set => this.RaiseAndSetIfChanged(ref _isStandaloneMode, value);
    }

}