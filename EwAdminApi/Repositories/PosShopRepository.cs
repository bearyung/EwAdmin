using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosShopRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosShopRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
    }

    // add a new method to get the shop list with pagination
    // code here
    public async Task<IEnumerable<Shop>> GetShopListAsync(int accountId, int page, int pageSize, int? shopId = null)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        
        // write the query to get the shop list with pagination
        // shopId is optional, if it is null, it should not be included in the query
        var query = @"
            SELECT [ShopId]
                  ,[AccountId]
                  ,[Name]
                  ,[AltName]
                  ,[Desc]
                  ,[AltDesc]
                  ,[AddressLine1]
                  ,[AddressLine2]
                  ,[AddressLine3]
                  ,[AddressLine4]
                  ,[AltAddressLine1]
                  ,[AltAddressLine2]
                  ,[AltAddressLine3]
                  ,[AltAddressLine4]
                  ,[Telephone]
                  ,[Fax]
                  ,[CurrencyCode]
                  ,[CurrencySymbol]
                  ,[Enabled]
                  ,[ShopCode]
              FROM [dbo].[Shop]
              where accountid = @AccountId
              AND (@ShopId IS NULL OR shopid = @ShopId)
              ORDER BY ShopId
              OFFSET @Offset ROWS
              FETCH NEXT @PageSize ROWS ONLY";

        // set the parameters, handle the case when shopId is null
        var parameters = new
        {
            AccountId = accountId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize,
            ShopId = shopId
        };


        if (db != null)
        {
            return await db.QueryAsync<Shop>(query, parameters).ConfigureAwait(false);
        }

        return [];
    }

    public async Task<Shop?> GetShopDetailAsync(int accountId, int shopId)
    {
        // get the connection from webadmin DB
        // get the shop details from pos DB
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT [ShopId]
                  ,[AccountId]
                  ,[Name]
                  ,[AltName]
                  ,[Desc]
                  ,[AltDesc]
                  ,[AddressLine1]
                  ,[AddressLine2]
                  ,[AddressLine3]
                  ,[AddressLine4]
                  ,[AltAddressLine1]
                  ,[AltAddressLine2]
                  ,[AltAddressLine3]
                  ,[AltAddressLine4]
                  ,[Telephone]
                  ,[Fax]
                  ,[CurrencyCode]
                  ,[CurrencySymbol]
                  ,[Enabled]
                  ,[ShopCode]
              FROM [dbo].[Shop]
              where accountid = @AccountId and shopid = @ShopId";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId
        };

        if (db != null)
        {
            return await db.QuerySingleOrDefaultAsync<Shop?>(query, parameters).ConfigureAwait(false);
        }

        return null;
    }
}