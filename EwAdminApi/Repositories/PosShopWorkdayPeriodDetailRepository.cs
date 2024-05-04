using Dapper;
using EwAdminApi.Models.Pos;
using EwAdminApi.Repositories.BaseClass;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosShopWorkdayPeriodDetailRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;
    
    public PosShopWorkdayPeriodDetailRepository(IConnectionService connectionService):base(connectionService)
    {
        _connectionService = connectionService;
    }
    
    // input: accountId, shopId, workdaydetailid, page, pageSize
    // output: list of ShopWorkdayPeriodDetail
    public async Task<IEnumerable<ShopWorkdayPeriodDetail>?> GetShopWorkdayPeriodDetailListAsync(
    int accountId, int shopId, int workdayDetailId, int page, int pageSize)
{
    using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
    var offset = (page - 1) * pageSize;
    var query = @"
    SELECT 
           a.[WorkdayPeriodDetailId]
          ,a.[AccountId]
          ,a.[ShopId]
          ,a.[WorkdayDetailId]
          ,a.[WorkdayPeriodId]
          ,a.[StartDatetime]
          ,a.[EndDatetime]
          ,a.[Enabled]
          ,b.[PeriodName]
      FROM [dbo].[ShopWorkdayPeriodDetail] a
      INNER JOIN [dbo].[ShopWorkdayPeriod] b
      ON a.AccountId = b.AccountId
      AND a.ShopId = b.ShopId
      AND a.WorkdayPeriodId = b.WorkdayPeriodId
      WHERE a.accountId = @AccountId 
      AND a.shopid = @ShopId 
      AND a.workdaydetailid = @WorkdayDetailId
      ORDER BY a.StartDatetime DESC
      OFFSET @Offset ROWS 
      FETCH NEXT @PageSize ROWS ONLY
      ";

    var parameters = new
    {
        AccountId = accountId,
        ShopId = shopId,
        WorkdayDetailId = workdayDetailId,
        Offset = offset,
        PageSize = pageSize
    };

    if (db != null)
        return await (db.QueryAsync<ShopWorkdayPeriodDetail>(query, parameters)).ConfigureAwait(false);
    else
        return new List<ShopWorkdayPeriodDetail>();
}
}