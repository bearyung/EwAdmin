using Dapper;
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class WebAdminBrandMasterRepository(IConnectionService connectionService) : PosRepositoryBase(connectionService)
{
    private readonly IConnectionService _connectionService = connectionService;
    
    public async Task<IEnumerable<BrandMaster>> GetBrandMasterListAsync
        (int page, int pageSize, int? companyId = null, int? brandId = null)
    {
        using var db = _connectionService.GetConnection();
        var offset = (page - 1) * pageSize;
        var query = @"
        SELECT brand.BrandId, brand.BrandName, company.CompanyId, company.CompanyName, region.RegionId, region.RegionName
            from webadmin_BrandMaster brand, webadmin_CompanyMaster company, webadmin_ResellerAreaMaster reseller, webadmin_RegionMaster region
        WHERE brand.CompanyId = company.CompanyId
            and company.ResellerAreaId = reseller.ResellerAreaId
            and reseller.RegionId = region.RegionId
            and brand.Enabled = 1
            and (@BrandId is null or brand.BrandId = @BrandId)
            and (@CompanyId is null or brand.CompanyId = @CompanyId)
        ORDER BY brand.CompanyId, brand.BrandId
        OFFSET @Offset ROWS 
        FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            Offset = offset,
            PageSize = pageSize,
            BrandId = brandId,
            CompanyId = companyId
        };

        if (db != null)
            return await db.QueryAsync<BrandMaster>(query, parameters).ConfigureAwait(false);
        else
            return new List<BrandMaster>();
    }
}