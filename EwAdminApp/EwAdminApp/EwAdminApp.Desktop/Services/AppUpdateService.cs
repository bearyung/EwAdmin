using System;
using System.Threading.Tasks;
using EwAdminApp.Services;
using ReactiveUI;
using Velopack;

namespace EwAdminApp.Desktop.Services;

public class AppUpdateService : ReactiveObject, IAppUpdateService
{
    // init update manager
    private UpdateManager _appUpdateManager;
    private UpdateInfo? _appUpdate;
    private int _progressValue;
    
    public UpdateManager AppUpdateManager
    {
        get => _appUpdateManager;
        set => this.RaiseAndSetIfChanged(ref _appUpdateManager, value);
    }
    
    public UpdateInfo? AppUpdate
    {
        get => _appUpdate;
        set => this.RaiseAndSetIfChanged(ref _appUpdate, value);
    }
    
    public int ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public AppUpdateService()
    {
        _appUpdateManager = new UpdateManager("https://everywarestorage.blob.core.windows.net/ewadminupdate/releases");
    }

    public async Task CheckForUpdates()
    {
        // log the method call
        Console.WriteLine($"{GetType().Name}: CheckForUpdates");
        
        try
        {
            // check for new version
            _appUpdate = await _appUpdateManager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (_appUpdate == null)
            {
                // log the message that no update is available
                Console.WriteLine("No update available");
            }
            else
            {
                Console.WriteLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
                Console.WriteLine($"This app version: {(AppUpdateManager.IsInstalled ? AppUpdateManager.CurrentVersion : "(n/a - not installed)")}");
                Console.WriteLine($"Update available: {AppUpdate?.TargetFullRelease.Version}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DownloadUpdates()
    {
        try
        {
            // check for new version
            _appUpdate = await _appUpdateManager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (_appUpdate == null)
            {
                // log the message that no update is available
                Console.WriteLine("No update available");
                return; // no update available
            }
            
            // download new version
            await _appUpdateManager.DownloadUpdatesAsync(_appUpdate, Progress).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ApplyUpdatesAndRestart()
    {
        try
        {
            // check for new version
            _appUpdate = await _appUpdateManager.CheckForUpdatesAsync().ConfigureAwait(false);
            if (_appUpdate == null)
            {
                // log the message that no update is available
                Console.WriteLine("No update available");
                return; // no update available
            }
            else
            {
                // log the new version
                Console.WriteLine($"{GetType().Name}: Update available: {_appUpdate.TargetFullRelease.Version}");
            }

            // download new version
            await _appUpdateManager.DownloadUpdatesAsync(_appUpdate).ConfigureAwait(false);

            // install new version and restart app
            _appUpdateManager.ApplyUpdatesAndRestart(_appUpdate);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void Progress(int percent)
    {
        ProgressValue = percent;
    }
}