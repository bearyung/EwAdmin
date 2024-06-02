using System;
using System.Reactive.Disposables;
using System.Threading;
using ReactiveUI;

namespace EwAdminApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    // add a property for Activator
    public ViewModelActivator Activator { get; } = new();

}