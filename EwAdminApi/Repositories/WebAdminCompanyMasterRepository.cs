using System.Data;
using Dapper;
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class WebAdminCompanyMasterRepository
{
    private readonly IConnectionService _connectionService;

    public WebAdminCompanyMasterRepository(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<IEnumerable<CompanyMaster>> GetCompanyMasterListAsync(int page, int pageSize, int? companyId = null)
    {
        using var db = _connectionService.GetConnection();
        var offset = (page - 1) * pageSize;
        var query = @"
        SELECT company.CompanyId, company.CompanyName, region.RegionId, region.RegionName
            from webadmin_CompanyMaster company, webadmin_ResellerAreaMaster reseller, webadmin_RegionMaster region
        WHERE company.ResellerAreaId = reseller.ResellerAreaId
            and reseller.RegionId = region.RegionId
            and company.Enabled = 1
            and (@CompanyId is null or company.CompanyId = @CompanyId)
        ORDER BY company.CompanyId
        OFFSET @Offset ROWS 
        FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            Offset = offset,
            PageSize = pageSize,
            CompanyId = companyId
        };

        if (db != null)
            return await db.QueryAsync<CompanyMaster>(query, parameters).ConfigureAwait(false);
        else
            return new List<CompanyMaster>();
    }
}