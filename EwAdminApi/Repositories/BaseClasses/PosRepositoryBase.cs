using System.Data;
using Dapper;
using EwAdminApi.Models.WebAdmin;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosRepositoryBase(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    protected async Task<IDbConnection?> GetPosDatabaseConnection(int brandId, int shopId)
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

        var result = await db.QuerySingleOrDefaultAsync<RegionMaster>(query, parameters).ConfigureAwait(false);

        if (result != null)
        {
            return _connectionService.GetConnection(result.DBServer, result.DBName, result.DBUsername,
                result.DBPassword);
        }

        return null;
    }
}