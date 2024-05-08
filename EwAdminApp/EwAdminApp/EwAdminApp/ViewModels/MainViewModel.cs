using System;
using EwAdminApp.Events;
using ReactiveUI;
using Splat;

namespace EwAdminApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _contentViewModel;

    public MainViewModel()
    {
        ContentViewModel = new LoginViewModel();
        
        var messageEventAggregator = Locator.Current.GetService<IEventAggregator>();
        
        messageEventAggregator?.GetEvent<MessageEvent>().Subscribe(OnMessageReceived);
    }
    
    public ViewModelBase? ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
    
    private void OnMessageReceived(MessageEvent msgEvent)
    {
        Console.WriteLine($"RECEIVED:{msgEvent.Message}");
    }
}