using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosShopWorkdayPeriodDetailRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosShopWorkdayPeriodDetailRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
    }

    // input: accountId, shopId, workdaydetailid, page, pageSize
    // output: list of ShopWorkdayPeriodDetail
    public async Task<IEnumerable<ShopWorkdayPeriodDetail>?> GetShopWorkdayPeriodDetailListAsync(
        int accountId, int shopId, int workdayDetailId, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
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
          ,a.[ModifiedDate]
          ,a.[ModifiedBy]
          ,b.[PeriodName]
      FROM [dbo].[ShopWorkdayPeriodDetail] a
      INNER JOIN [dbo].[ShopWorkdayPeriod] b
      ON a.AccountId = b.AccountId
      AND a.ShopId = b.ShopId
      AND a.WorkdayPeriodId = b.WorkdayPeriodId
      WHERE a.accountId = @AccountId 
      AND a.shopid = @ShopId 
      AND a.workdaydetailid = @WorkdayDetailId
      ORDER BY a.StartDatetime
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

    // UpdateShopWorkdayPeriodDetailListAsync
    // input: ShopWorkdayPeriodDetail
    // output: updated ShopWorkdayPeriodDetail
    public async Task<ShopWorkdayPeriodDetail?> UpdateShopWorkdayPeriodDetailAsync(
        ShopWorkdayPeriodDetail shopWorkdayPeriodDetail)
    {
        using var db = await GetPosDatabaseConnectionByAccount(shopWorkdayPeriodDetail.AccountId)
            .ConfigureAwait(false);
        var query = @"
        UPDATE [dbo].[ShopWorkdayPeriodDetail]
        SET [StartDatetime] = @StartDatetime
           ,[EndDatetime] = @EndDatetime
           ,[Enabled] = @Enabled
           ,[ModifiedDate] = GETDATE()
           ,[ModifiedBy] = @ModifiedBy
        WHERE AccountId = @AccountId
        AND ShopId = @ShopId
        AND WorkdayPeriodDetailId = @WorkdayPeriodDetailId
        ";

        if (db != null)
        {
            // if the update is successful, get the updated shop workday period detail from DB and return that to user
            // otherwise, return null
            var result = await db.ExecuteAsync(query, shopWorkdayPeriodDetail).ConfigureAwait(false);
            if (result > 0)
            {
                return await GetShopWorkdayPeriodDetailAsync(shopWorkdayPeriodDetail.AccountId,
                    shopWorkdayPeriodDetail.ShopId, shopWorkdayPeriodDetail.WorkdayPeriodDetailId).ConfigureAwait(false);
            }
        }

        return null;
    }

    private async Task<ShopWorkdayPeriodDetail?> GetShopWorkdayPeriodDetailAsync(int accountId, int shopId,
        int workdayPeriodDetailId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
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
          ,a.[ModifiedDate]
          ,a.[ModifiedBy]
          ,b.[PeriodName]
      FROM [dbo].[ShopWorkdayPeriodDetail] a
      INNER JOIN [dbo].[ShopWorkdayPeriod] b
      ON a.AccountId = b.AccountId
      AND a.ShopId = b.ShopId
      AND a.WorkdayPeriodId = b.WorkdayPeriodId
      WHERE a.accountId = @AccountId 
      AND a.shopid = @ShopId 
      AND a.workdayperioddetailid = @WorkdayPeriodDetailId
      ";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            WorkdayPeriodDetailId = workdayPeriodDetailId
        };

        if (db != null)
            return await (db.QuerySingleOrDefaultAsync<ShopWorkdayPeriodDetail>(query, parameters))
                .ConfigureAwait(false);
        else
            return null;
    }
}