using System.Data;
using Dapper;
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosRepositoryBase(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    protected async Task<IDbConnection?> GetPosDatabaseConnection(int brandId)
    {
        using var db = _connectionService.GetConnection();
        var query = @"
            select region.DBServer, region.DBName, region.DBUsername, region.DBPassword
            from webadmin_BrandMaster brand, webadmin_CompanyMaster company, webadmin_ResellerAreaMaster reseller, webadmin_RegionMaster region
            WHERE brand.CompanyId = company.CompanyId
            and company.ResellerAreaId = reseller.ResellerAreaId
            and reseller.RegionId = region.RegionId
            and brandId = @BrandId";

        var parameters = new
        {
            BrandId = brandId
        };

        if (db != null)
        {
            var result = await db.QuerySingleOrDefaultAsync<RegionMaster>(query, parameters).ConfigureAwait(false);

            if (result is { DbServer: not null, DbName: not null, DbUsername: not null, DbPassword: not null })
                return _connectionService.GetConnection(result.DbServer, result.DbName,
                    result.DbUsername,
                    result.DbPassword);
        }

        return null;
    }
}