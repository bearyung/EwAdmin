using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.ReactiveUI;
using EwAdminApp.ViewModels;
using ReactiveUI;

namespace EwAdminApp.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        
        this.WhenActivated((disposables) =>
        {
            // log when the LoginView is activated
            Console.WriteLine($"{GetType().Name} is being activated.");
            
            var inputPane = TopLevel.GetTopLevel(this)?.InputPane;
            
            if (inputPane != null)
            {
                // Subscribe to the inputPane's StateChanged event
                inputPane.StateChanged += InputPaneOnStateChanged;
            }
            
            
            Disposable.Create(() =>
                {
                    Console.WriteLine($"{GetType().Name} is being deactivated.");
                    
                    if (inputPane != null)
                    {
                        // log when the inputPane's StateChanged event is unsubscribed
                        Console.WriteLine($"{GetType().Name}: Unsubscribing from inputPane.StateChanged");
                        inputPane.StateChanged -= InputPaneOnStateChanged;
                    }
                })
                .DisposeWith(disposables);
        });
    }

    private void InputPaneOnStateChanged(object? sender, InputPaneStateEventArgs e)
    {
        Console.WriteLine($"{GetType().Name}: InputPaneOnStateChanged");
        
        switch (e.NewState)
        {
            case InputPaneState.Open:
                var screenHeight = TopLevel.GetTopLevel(this)?.Height ?? 0;
                var containerHeight = MainViewDialogHost.Bounds.Height;
                var containerTop = MainViewDialogHost.Bounds.Top;
                var keyboardHeight = e.EndRect.Height;
                var focusedControl = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
                if (focusedControl != null)
                {
                    // Assuming MainScrollViewer is the direct or an indirect parent of the TextBox
                    var relativePoint = focusedControl.TranslatePoint(new Point(0, 0), MainViewUserControl);

                    if (relativePoint.HasValue)
                    {
                        double controlBottom = relativePoint.Value.Y + focusedControl.Bounds.Height;
                        double keyboardTop = screenHeight - keyboardHeight;  // Calculate or get these values as needed

                        // Check if the TextBox bottom is below the top of the keyboard
                        if (controlBottom > keyboardTop)
                        {
                            Console.WriteLine(MainViewUserControl.Bounds.Height);
                            MainViewDockPanel.Margin = new Thickness(0,0,0,keyboardHeight - (screenHeight - containerHeight) + containerTop);
                            MainScrollViewer.Offset = new Vector(MainScrollViewer.Offset.X, controlBottom - keyboardTop + MainScrollViewer.Offset.Y);
                        }
                    }
                }
                break;
            case InputPaneState.Closed:
                MainViewDockPanel.Margin = new Thickness(0);
                MainScrollViewer.Offset = new Vector(MainScrollViewer.Offset.X, 0);
                break;
        }
        
    }
    
}