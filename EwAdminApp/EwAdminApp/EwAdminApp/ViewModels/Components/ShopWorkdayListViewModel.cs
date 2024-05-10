using System.Reactive;
using ReactiveUI;

namespace EwAdminApp.ViewModels.Components;

public class ShopWorkdayListViewModel : ViewModelBase
{
    // using ReactiveUI for all implementations
    // Add a property named "Title" of type string
    // code here
    private string? _title;
    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    
    public ReactiveCommand<Unit, string> ConfirmSelectionCommand { get; }

    public ShopWorkdayListViewModel()
    {
        Title = "Shop Workday List";
        ConfirmSelectionCommand = ReactiveCommand.Create(() => "Shop Workday List Selected");
    }
}