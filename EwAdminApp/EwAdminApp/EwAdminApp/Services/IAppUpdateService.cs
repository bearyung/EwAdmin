using System.Threading.Tasks;
using Velopack;

namespace EwAdminApp.Services;

public interface IAppUpdateService
{
    public UpdateManager AppUpdateManager { get; set; }
    public UpdateInfo? AppUpdate { get; set; }
    
    public int ProgressValue { get; set; }
    Task CheckForUpdates();
    Task DownloadUpdates();
    Task ApplyUpdatesAndRestart();
}