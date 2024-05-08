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

    public async Task<IEnumerable<CompanyMaster>> GetCompanyMasterListAsync(int page, int pageSize)
    {
        using var db = _connectionService.GetConnection();
        var offset = (page - 1) * pageSize;
        var query = @"
        SELECT companyId, companyName 
        FROM webadmin_companymaster
        ORDER BY companyId
        OFFSET @Offset ROWS 
        FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            Offset = offset,
            PageSize = pageSize
        };

        if (db != null)
            return await db.QueryAsync<CompanyMaster>(query, parameters).ConfigureAwait(false);
        else
            return new List<CompanyMaster>();
    }
}