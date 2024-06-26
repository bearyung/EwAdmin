using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosPaymentMethodRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosPaymentMethodRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
    }

    // input: accountId, shopId, page, pageSize
    // output: list of PaymentMethod
    public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodListAsync(int accountId, int shopId, int page,
        int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;
        var query = @"
            SELECT 
                 a.[PaymentMethodId]
                ,a.[PaymentMethodCode]
                ,a.[PaymentMethodName]
                ,a.[DisplayIndex]
                ,a.[Enabled]
                ,a.[AccountId]
                ,a.[LinkedGateway]
                ,a.[IsNonSalesPayment]
                ,a.[IsCashPayment]
            FROM [dbo].[PaymentMethod] a
            LEFT JOIN 
            ( select * from [dbo].[PaymentMethodShopDetail] where AccountId = @AccountId and shopId = @ShopId ) as b
            ON a.AccountId = b.AccountId 
                and a.PaymentMethodId = b.PaymentMethodId
            WHERE a.accountId = @AccountId
            AND a.Enabled = 1
            and (b.Enabled = 1 or b.Enabled is null)
          ORDER BY a.PaymentMethodId
          OFFSET @Offset ROWS 
          FETCH NEXT @PageSize ROWS ONLY
          ";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            Offset = offset,
            PageSize = pageSize
        };

        if (db != null)
        {
            return await (db.QueryAsync<PaymentMethod>(query, parameters)).ConfigureAwait(false);
        }

        return [];
    }
}