using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using EwAdminApp.Services;
using ReactiveUI;
using Splat;
using UnitsNet;
using Velopack;

namespace EwAdminApp.ViewModels.Components;

public class SettingsCheckForUpdatesViewModel : ViewModelBase
{
    // add a property of CurrentVersion
    private string? _currentVersion;
    public string? CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }
    
    // add a property of UpdaterVersion
    private string? _updaterVersion;
    public string? UpdaterVersion
    {
        get => _updaterVersion;
        set => this.RaiseAndSetIfChanged(ref _updaterVersion, value);
    }
    
    // add a property of LatestVersion
    private string? _latestVersion;
    public string? LatestVersion
    {
        get => _latestVersion;
        set => this.RaiseAndSetIfChanged(ref _latestVersion, value);
    }
    
    private string? _fileSize;
    public string? FileSize
    {
        get => _fileSize;
        set => this.RaiseAndSetIfChanged(ref _fileSize, value);
    }
    
    // add a property of IsBusy
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    // add a property of HasUpdates
    private bool _hasUpdates;
    public bool HasUpdates
    {
        get => _hasUpdates;
        set => this.RaiseAndSetIfChanged(ref _hasUpdates, value);
    }
    
    // add a property of CanDownloadUpdates
    private bool _canDownloadUpdates;
    public bool CanDownloadUpdates
    {
        get => _canDownloadUpdates;
        set => this.RaiseAndSetIfChanged(ref _canDownloadUpdates, value);
    }
    
    // add a property of CanApplyUpdates
    private bool _canApplyUpdates;
    public bool CanApplyUpdates
    {
        get => _canApplyUpdates;
        set => this.RaiseAndSetIfChanged(ref _canApplyUpdates, value);
    }
    
    private int _progressValue;
    public int ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }
    
    private string? _deltaAssetListString;
    public string? DeltaAssetListString
    {
        get => _deltaAssetListString;
        set => this.RaiseAndSetIfChanged(ref _deltaAssetListString, value);
    }
    
    // add a ReactiveCommand property of CheckForUpdatesCommand
    public ReactiveCommand<Unit, Unit>? CheckForUpdatesCommand { get; }
    
    // add a ReactiveCommand property of DownloadUpdatesCommand
    public ReactiveCommand<Unit, Unit>? DownloadUpdatesCommand { get; }
    
    // add a ReactiveCommand property of ApplyUpdatesCommand
    public ReactiveCommand<Unit, Unit>? ApplyUpdatesCommand { get; }
    
    public SettingsCheckForUpdatesViewModel()
    {
        // get the AppUpdateService from Locator
        var appUpdateService = Locator.Current.GetService<IAppUpdateService>();
        
        // if the appUpdateService is null, return
        if (appUpdateService == null)
        {
            // log the error about the service is unavailable
            Console.WriteLine("AppUpdateService is not available");
            return;
        }
        else
        {
            // log the service is available
            Console.WriteLine("AppUpdateService is available");
            Console.WriteLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
            Console.WriteLine($"This app version: {(appUpdateService.AppUpdateManager.IsInstalled ? appUpdateService.AppUpdateManager.CurrentVersion : "(n/a - not installed)")}");
            
            CurrentVersion = appUpdateService.AppUpdateManager.CurrentVersion?.ToString();
            UpdaterVersion = VelopackRuntimeInfo.VelopackNugetVersion.ToString();
            
            // set the properties of HasUpdates, CanDownloadUpdates, CanApplyUpdates and FileSize
            UpdateStatus();
        }
        
        // init the CheckForUpdatesCommand
        // use ReactiveCommand.CreateFromTask to create a command that will execute the CheckForUpdates method of the appUpdateService if IsBusy is false
        // check the appUpdateService.AppUpdate.TargetFullRelease.Version after the command is executed
        // code here
        CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(appUpdateService.CheckForUpdates);
        
        // init the DownloadUpdatesCommand
        DownloadUpdatesCommand = ReactiveCommand.CreateFromTask(appUpdateService.DownloadUpdates);
        
        // init the ApplyUpdatesCommand
        ApplyUpdatesCommand = ReactiveCommand.CreateFromTask(appUpdateService.ApplyUpdatesAndRestart);
        
        
        this.WhenActivated(disposables =>
        {
            // log when the viewmodel is activated
            Console.WriteLine($"{GetType().Name} activated");

            // set the IsBusy to true when the CheckForUpdatesCommand or DownloadUpdatesCommand or ApplyUpdatesCommand is executing
            CheckForUpdatesCommand.IsExecuting
                .Merge(DownloadUpdatesCommand.IsExecuting)
                .Merge(ApplyUpdatesCommand.IsExecuting)
                .Select(x => x ? 1 : -1)
                .Scan(0, (acc, x) => acc + x)
                .Select(x => x > 0)
                .ToProperty(this, x => x.IsBusy)
                .DisposeWith(disposables);
            
            // catch the exception when the CheckForUpdatesCommand is executed
            CheckForUpdatesCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine("Failed to check for updates");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);
            
            // catch the exception when the DownloadUpdatesCommand is executed
            DownloadUpdatesCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine("Failed to download updates");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);
            
            // catch the exception when the ApplyUpdatesCommand is executed
            ApplyUpdatesCommand.ThrownExceptions
                .Subscribe(ex =>
                {
                    Console.WriteLine("Failed to apply updates");
                    Console.WriteLine(ex.Message);
                })
                .DisposeWith(disposables);
            
            // subscribe to the CheckForUpdatesCommand
            // set the CurrentVersion, LatestVersion, and FileSize properties
            CheckForUpdatesCommand
                .Subscribe(_ =>
                {
                    HasUpdates = appUpdateService.AppUpdate != null;
                    CanDownloadUpdates = appUpdateService is { AppUpdate: not null, AppUpdateManager.IsUpdatePendingRestart: false };
                    
                    if (appUpdateService.AppUpdate != null)
                    {
                        UpdateStatus();
                    }
                })
                .DisposeWith(disposables);
            
            // subscribe to the DownloadUpdatesCommand
            // set the CanApplyUpdates property
            DownloadUpdatesCommand
                .Subscribe(_ =>
                {
                    UpdateStatus();
                })
                .DisposeWith(disposables);
            
            // watch for the value change of appUpdateService.ProgressValue property
            // set the ProgressValue property
            appUpdateService.WhenAnyValue(x => x.ProgressValue)
                .Subscribe(x => ProgressValue = x)
                .DisposeWith(disposables);
            
            // console log when the viewmodel is deactivated
            Disposable.Create(() => Console.WriteLine($"{GetType().Name} deactivated"))
                .DisposeWith(disposables);
        });
        
    }

    private void UpdateStatus()
    {
        try
        {
            // get the AppUpdateService from Locator
            var appUpdateService = Locator.Current.GetService<IAppUpdateService>();
        
            if(appUpdateService == null) return;

            DeltaAssetListString = string.Empty;

            HasUpdates = appUpdateService.AppUpdate != null;
            CanDownloadUpdates = appUpdateService is { AppUpdate: not null, AppUpdateManager.IsUpdatePendingRestart: false };
            CanApplyUpdates = appUpdateService.AppUpdateManager.IsUpdatePendingRestart;
            FileSize = Information.FromBytes(appUpdateService.AppUpdate?.TargetFullRelease.Size ?? 0).Mebibytes.ToString("0.00") + "MB";
            DeltaAssetListString += $"Base: {appUpdateService.AppUpdate?.BaseRelease?.Version.ToString()}";
            LatestVersion = appUpdateService.AppUpdate?.TargetFullRelease.Version.ToString();

            if (appUpdateService.AppUpdate != null)
            {
                foreach (var deltaAsset in appUpdateService.AppUpdate.DeltasToTarget)
                {
                    DeltaAssetListString += $"\n Delta: {deltaAsset.Version.ToString()}\t\t{Information.FromBytes(deltaAsset.Size).Mebibytes:0.00}MB";
                }
            }
            
            DeltaAssetListString += $"\nTarget: {appUpdateService.AppUpdate?.TargetFullRelease.Version.ToString()}";
        }
        catch (Exception )
        {
            Console.WriteLine();
            throw;
        }
    }
}