using Dapper;
using EwAdminApi.Models.Pos;
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
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);

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
}