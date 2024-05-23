using Dapper;
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class WebAdminRegionMasterRepository(IConnectionService connectionService) : PosRepositoryBase(connectionService)
{
    // add a new method to get the region list with pagination
    public async Task<IEnumerable<RegionMaster>> GetRegionListAsync(int accountId, int page, int pageSize, int? regionId = null)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        
        // write the query to get the region list with pagination
        // regionId is optional, if it is null, it should not be included in the query
        var query = @"
            SELECT [RegionId]
                  ,[AccountId]
                  ,[RegionName]
                  ,[AltRegionName]
                  ,[RegionCode]
                  ,[Enabled]
              FROM [dbo].[RegionMaster]
              where accountid = @AccountId
              AND (@RegionId IS NULL OR regionid = @RegionId)
              ORDER BY RegionId
              OFFSET @Offset ROWS
              FETCH NEXT @PageSize ROWS ONLY";

        // set the parameters, handle the case when regionId is null
        var parameters = new
        {
            AccountId = accountId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize,
            RegionId = regionId
        };

        // execute the query and return the result
        if (regionId == null)
        {
            return await db.QueryAsync<RegionMaster>(query, parameters).ConfigureAwait(false);
        }
        
        return Enumerable.Empty<RegionMaster>();
        
    }
}