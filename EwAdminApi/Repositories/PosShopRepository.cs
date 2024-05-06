using Dapper;
using EwAdminApi.Models.Pos;
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

    public async Task<Shop?> GetShopDetailAsync(int accountId, int shopId)
    {
        // get the connection from webadmin DB
        // get the shop details from pos DB
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
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