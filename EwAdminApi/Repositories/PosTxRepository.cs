using Dapper;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosTxRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;
    public PosTxRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
    }
    
    /// <summary>
    /// Check if there are any transactions in the given workday detail id
    /// returns the count of transactions
    /// return -1 if there is an error
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="workdayDetailId"></param>
    /// <returns>An integer representing the count of transactions for the given workday detail id, or -1 if there is an error.</returns>
    public async Task<int> GetTxCountInWorkdayDetailIdAsync(int accountId, int shopId, int workdayDetailId)
    {
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
        var query = @"
            select count(1) from TxSalesHeader a 
            where a.accountid = @AccountId and a.shopid = @ShopId
            and (
                WorkdayPeriodDetailId in
                    (select WorkdayPeriodDetailId from ShopWorkdayPeriodDetail where AccountId = a.AccountId and ShopId = a.ShopId and WorkdayDetailId = @WorkdayDetailId)
                or StartWorkdayPeriodDetailId in 
                    (select WorkdayPeriodDetailId from ShopWorkdayPeriodDetail where AccountId = a.AccountId and ShopId = a.ShopId and WorkdayDetailId = @WorkdayDetailId)
            )";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            WorkdayDetailId = workdayDetailId
        };
        if (db != null)
            return await db.QuerySingleAsync<int>(query, parameters).ConfigureAwait(false);
        else
            return -1;
    }
    
}