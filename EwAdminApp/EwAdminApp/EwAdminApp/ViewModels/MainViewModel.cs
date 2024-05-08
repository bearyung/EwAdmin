using ReactiveUI;

namespace EwAdminApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;

    public MainViewModel()
    {
        ContentViewModel = new LoginViewModel();
    }
    
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
}