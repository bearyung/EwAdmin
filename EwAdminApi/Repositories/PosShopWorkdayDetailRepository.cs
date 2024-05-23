using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosShopWorkdayDetailRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosShopWorkdayDetailRepository(IConnectionService connectionService):base(connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<IEnumerable<ShopWorkdayDetail>?> GetShopWorkdayDetailListAsync(
        int accountId, int shopId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);

        // if startDate is null, return the latest 10 records,
        // otherwise return the records between startDate and endDate with pagination
        if (startDate == null)
        {
            var query = @"
            SELECT TOP 10
                   [WorkdayDetailId]
                  ,[AccountId]
                  ,[WorkdayHeaderId]
                  ,[ShopId]
                  ,[OpenDatetime]
                  ,[CloseDatetime]
                  ,[IsClosed]
                  ,[Enabled]
                  ,[CreatedDate]
                  ,[CreatedBy]
                  ,[ModifiedDate]
                  ,[ModifiedBy]
              FROM [dbo].[ShopWorkdayDetail]
              WHERE accountId = @AccountId and shopid = @ShopId
              ORDER BY OpenDatetime DESC
              ";

            var parameters = new
            {
                AccountId = accountId,
                ShopId = shopId
            };

            if (db != null)
                return await (db.QueryAsync<ShopWorkdayDetail>(query, parameters)).ConfigureAwait(false);
            else
                return new List<ShopWorkdayDetail>();
        }
        else
        {
            // if enddate is null, set it to the startdate + 1 day
            endDate ??= startDate.Value.AddDays(1);
            
            var offset = (page - 1) * pageSize;
            var query = @"
            SELECT 
                   [WorkdayDetailId]
                  ,[AccountId]
                  ,[WorkdayHeaderId]
                  ,[ShopId]
                  ,[OpenDatetime]
                  ,[CloseDatetime]
                  ,[IsClosed]
                  ,[Enabled]
                  ,[CreatedDate]
                  ,[CreatedBy]
                  ,[ModifiedDate]
                  ,[ModifiedBy]
              FROM [dbo].[ShopWorkdayDetail]
              WHERE accountId = @AccountId and shopid = @ShopId
              and OpenDatetime >= @StartDate and OpenDatetime <= @EndDate
              ORDER BY OpenDatetime
              OFFSET @Offset ROWS 
              FETCH NEXT @PageSize ROWS ONLY
              ";

            var parameters = new
            {
                AccountId = accountId,
                ShopId = shopId,
                StartDate = startDate,
                EndDate = endDate,
                Offset = offset,
                PageSize = pageSize
            };

            if (db != null)
                return await (db.QueryAsync<ShopWorkdayDetail>(query, parameters)).ConfigureAwait(false);
            else
                return new List<ShopWorkdayDetail>();
        }
    }
    
    // UpdateShopWorkdayDetailAsync
    // input: ShopWorkdayDetail
    //          only OpenDatetime, CloseDatetime, IsClosed, Enabled, ModifiedBy  is being updated
    //          ModifiedDate is updated automatically with current datetime
    // output: updated ShopWorkdayDetail
    public async Task<ShopWorkdayDetail?> UpdateShopWorkdayDetailAsync(ShopWorkdayDetail shopWorkdayDetail)
    {
        using var db = await GetPosDatabaseConnectionByAccount(shopWorkdayDetail.AccountId)
            .ConfigureAwait(false);
        var query = @"
        UPDATE [dbo].[ShopWorkdayDetail]
        SET [OpenDatetime] = @OpenDatetime
           ,[CloseDatetime] = @CloseDatetime
           ,[IsClosed] = @IsClosed
           ,[Enabled] = @Enabled
           ,[ModifiedDate] = GETDATE()
           ,[ModifiedBy] = @ModifiedBy
        WHERE AccountId = @AccountId
        AND ShopId = @ShopId
        AND WorkdayDetailId = @WorkdayDetailId
        ";

        if (db != null)
            await db.ExecuteAsync(query, shopWorkdayDetail).ConfigureAwait(false);

        return shopWorkdayDetail;
    }
    
    // DeleteShopWorkdayDetailAsync
    // input: ShopWorkdayDetail
    // output: boolean indicating success
    public async Task<bool> DeleteShopWorkdayDetailAsync(ShopWorkdayDetail shopWorkdayDetail)
    {
        // delete the shop workday detail with the given workday detail id
        // delete the shop workday period details with the same workday detail id
        // the shop workday period details are deleted within the same transaction
        using var db = await GetPosDatabaseConnectionByAccount(shopWorkdayDetail.AccountId)
            .ConfigureAwait(false);
        db?.Open();
        using var transaction = db?.BeginTransaction();
        var query = @"
        DELETE FROM [dbo].[ShopWorkdayDetail]
        WHERE AccountId = @AccountId
        AND ShopId = @ShopId
        AND WorkdayDetailId = @WorkdayDetailId
        ";

        await db!.ExecuteAsync(query, shopWorkdayDetail, transaction).ConfigureAwait(false);

        var query2 = @"
            DELETE FROM [dbo].[ShopWorkdayPeriodDetail]
            WHERE AccountId = @AccountId
            AND ShopId = @ShopId
            AND WorkdayDetailId = @WorkdayDetailId
            ";

        await db!.ExecuteAsync(query2, shopWorkdayDetail, transaction).ConfigureAwait(false);

        transaction?.Commit();
        

        return true;
    }
}