using System.Collections.Concurrent;
using EwAdminApp.Events;
using EwAdminApp.Models;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace EwAdminApp.ViewModels.Components;

public class ActionStatusMonitorViewModel : ViewModelBase
{
    // in this view model, we will have a thread-safe ConcurrentQueueof action status messages that will be displayed
    // also a counter of thread-safe ExecutingCount to show how many actions are currently executing

    // initialize the properties
    private ConcurrentQueue<ActionStatus> _actionStatusMessages = new();

    public ConcurrentQueue<ActionStatus> ActionStatusMessages
    {
        get => _actionStatusMessages;
        set => this.RaiseAndSetIfChanged(ref _actionStatusMessages, value);
    }
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    // init the property of TotalExecutingCommandCount
    private int _totalExecutingCommandCount;
    public int TotalExecutingCommandCount
    {
        get => _totalExecutingCommandCount;
        set => this.RaiseAndSetIfChanged(ref _totalExecutingCommandCount, value);
    }
    
    // init the property of IsBackgroundCommandExecuting
    private bool _isBackgroundCommandExecuting;
    public bool IsBackgroundCommandExecuting
    {
        get => _isBackgroundCommandExecuting;
        set => this.RaiseAndSetIfChanged(ref _isBackgroundCommandExecuting, value);
    }
    
    // init the property of LastActionStatusMessage
    private ActionStatus? _lastActionStatusMessage;
    
    public ActionStatus? LastActionStatusMessage
    {
        get => _lastActionStatusMessage;
        set => this.RaiseAndSetIfChanged(ref _lastActionStatusMessage, value);
    }

    // constructor
    public ActionStatusMonitorViewModel()
    {
        // initialize the properties
        ActionStatusMessages = new ConcurrentQueue<ActionStatus>();
        IsBusy = false;

        this.WhenActivated((disposable) =>
        {
            // log the activation of viewmodel
            Console.WriteLine($"{GetType().Name} activated");
            
            // subscribe to the MessageBus.Current.Listen<ActionStatusMessageEvent>()
            // and update the properties accordingly
            MessageBus.Current.Listen<ActionStatusMessageEvent>()
                .Subscribe(actionStatusMessage =>
                {
                    // log the actionStatusMessage
                    Console.WriteLine(
                        $"{GetType().Name}: ActionStatusMessage: {actionStatusMessage.ActionStatusMessage.ActionStatusEnum} - {actionStatusMessage.ActionStatusMessage.Message}");

                    // switch on the ActionStatusEnum of the actionStatusMessage
                    switch (actionStatusMessage.ActionStatusMessage.ActionStatusEnum)
                    {
                        case ActionStatus.StatusEnum.Success:
                        case ActionStatus.StatusEnum.Error:
                        case ActionStatus.StatusEnum.Info:
                        case ActionStatus.StatusEnum.Executing:
                        case ActionStatus.StatusEnum.Completed:
                        case ActionStatus.StatusEnum.Interrupted:
                            // add the actionStatusMessage to the ActionStatusMessages
                            ActionStatusMessages.Enqueue(actionStatusMessage.ActionStatusMessage);
                            
                            // update the LastActionStatusMessage property
                            LastActionStatusMessage = actionStatusMessage.ActionStatusMessage;
                            break;
                    }
                })
                .DisposeWith(disposable);
            
            // subscribe to the MessageBus.Current.Listen<ExecutingCommandsCountEvent>()
            // and update the TotalExecutingCommandCount property
            MessageBus.Current.Listen<ExecutingCommandsCountEvent>()
                .Subscribe(executingCommandsCountEvent =>
                {
                    // log the ExecutingCommandsCountEvent
                    Console.WriteLine($"{GetType().Name}: ExecutingCommandsCountEvent: {executingCommandsCountEvent.ExecutingCommandsCount}");

                    // update the TotalExecutingCommandCount property
                    TotalExecutingCommandCount = executingCommandsCountEvent.ExecutingCommandsCount;
                    
                    // update the IsBackgroundCommandExecuting property
                    IsBackgroundCommandExecuting = TotalExecutingCommandCount > 0;
                })
                .DisposeWith(disposable);
            
            // log the deactivation of viewmodel
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} is being deactivated."))
                .DisposeWith(disposable);
        });
    }
}