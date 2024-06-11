using Dapper;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosSyncRepository(IConnectionService connectionService, IHttpContextAccessor? httpContextAccessor = null)
    : PosRepositoryBase(connectionService, httpContextAccessor)
{
    // Implement method to stop data sync by setting the syncReady of sync_groups table to N
    // code here
    public async Task<bool> StopDataSync(int regionId)
    {
        using var db = await GetPosDatabaseConnectionByRegion(regionId).ConfigureAwait(false);
        if (db == null) return false;
        var query = @"
                UPDATE sync_groups
                SET syncReady = 'N'
                WHERE syncReady = 'Y'";
        return await db.ExecuteAsync(query).ConfigureAwait(false) > 0;

    }
    
    // Implement method to resume data sync by setting the syncReady of sync_groups table to Y
    // code here
    public async Task<bool> ResumeDataSync(int regionId)
    {
        using var db = await GetPosDatabaseConnectionByRegion(regionId).ConfigureAwait(false);
        if (db == null) return false;
        var query = @"
                UPDATE sync_groups
                SET syncReady = 'Y'
                WHERE syncReady = 'N'";
        return await db.ExecuteAsync(query).ConfigureAwait(false) > 0;
    }
    
    // Implement method to get the status of data sync by getting the syncReady of sync_groups table
    // code here
    public async Task<string> GetDataSyncStatus(int regionId)
    {
        using var db = await GetPosDatabaseConnectionByRegion(regionId).ConfigureAwait(false);
        if (db == null) return "Error";
        var query = @"
                SELECT syncReady
                FROM sync_groups";
        return await db.QueryFirstOrDefaultAsync<string>(query).ConfigureAwait(false);
    }
}