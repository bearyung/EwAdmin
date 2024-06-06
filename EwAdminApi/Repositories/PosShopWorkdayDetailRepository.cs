using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosShopWorkdayDetailRepository : PosShopWorkdayDetailRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosShopWorkdayDetailRepository(IConnectionService connectionService,
        IHttpContextAccessor httpContextAccessor) : base(connectionService, httpContextAccessor)
    {
        _connectionService = connectionService;
    }

    public async Task<IEnumerable<ShopWorkdayDetail>> GetShopWorkdayDetailListAsync(
        int accountId, int shopId, DateTime? startDate, DateTime? endDate,
        int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);

        var offset = (page - 1) * pageSize;
        
        // as the OpenDatetime may not be 00:00:00, so if user input the endDate as 2024-11-30
        // the endDate should be LESS THAN 2024-12-01 00:00:00
        // so we need to add 1 day to the endDate
        endDate = endDate?.AddDays(1);

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
      WHERE AccountId = @AccountId AND ShopId = @ShopId
      AND (@StartDate IS NULL OR OpenDatetime >= @StartDate)
      AND (@EndDate IS NULL OR OpenDatetime < @EndDate)
      ORDER BY OpenDatetime DESC
      OFFSET @Offset ROWS 
      FETCH NEXT @PageSize ROWS ONLY";

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
            return await db.QueryAsync<ShopWorkdayDetail>(query, parameters).ConfigureAwait(false);
        else
            return new List<ShopWorkdayDetail>();
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

        // only OpenDatetime, CloseDatetime, IsClosed, Enabled, ModifiedDate, ModifiedBy to be updated using BuildUpdateQuery
        var (query, parameters) = BuildUpdateQuery(shopWorkdayDetail, [
            nameof(ShopWorkdayDetail.OpenDatetime),
            nameof(ShopWorkdayDetail.CloseDatetime),
            nameof(ShopWorkdayDetail.IsClosed),
            nameof(ShopWorkdayDetail.Enabled)
        ], [
            nameof(ItemCategory.ModifiedDate),
            nameof(ItemCategory.ModifiedBy),
        ]);

        if (db != null)
            await db.ExecuteAsync(query, parameters).ConfigureAwait(false);

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