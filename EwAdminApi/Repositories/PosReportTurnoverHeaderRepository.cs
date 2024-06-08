using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;
using Microsoft.Identity.Client;

public class PosReportTurnoverHeaderRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;

    public PosReportTurnoverHeaderRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
    }
    
    // input: accountId, shopId, page, pageSize
    // optional input: workdayOpenDatetimeGte, workdayOpenDatetimeLte
    // output: list of ReportTurnoverHeader
    public async Task<IEnumerable<ReportTurnoverHeader>> GetReportTurnoverHeaderListAsync(
        int accountId, int shopId, int page, int pageSize,
        DateTime? workdayOpenDatetimeGte = null, DateTime? workdayOpenDatetimeLte = null)
    {
        // if workdayOpenDatetimeGte is not null, add condition to query
        // if workdayOpenDatetimeLte is not null, add condition to query

        // as the actural workdayOpenDatetime may not be exactly 00:00:00,
        // it is better to use the condition of < workdayOpenDatetimeLte.AddDays(1)

        workdayOpenDatetimeLte = workdayOpenDatetimeLte?.AddDays(1);

        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;
        var query = @"
            SELECT 
                 a.[ReportTurnoverHeaderId]
                ,a.[AccountId]
                ,a.[ShopId]
                ,a.[ClearingDatetime]
                ,a.[LastPrintDatetime]
                ,a.[LastPrintCount]
                ,a.[LastPrintBy]
                ,a.[DayTurnover]
                ,a.[DayDiscount]
                ,a.[DayServiceFee]
                ,a.[DayRounding]
                ,a.[DayNetTurnoverAmount]
                ,a.[DayNetTurnoverTxCount]
                ,a.[UncloseTxCount]
                ,a.[UncloseTxAmount]
                ,a.[WorkdayDetailId]
                ,a.[WorkdayOpenDatetime]
                ,a.[WorkdayCloseDatetime]
                ,a.[Enabled]
                ,a.[CreatedDate]
                ,a.[CreatedBy]
                ,a.[ModifiedDate]
                ,a.[ModifiedBy]
            FROM [dbo].[ReportTurnoverHeader] a
            WHERE a.accountId = @AccountId
            AND a.shopid = @ShopId
            AND (@WorkdayOpenDatetimeGte IS NULL OR a.WorkdayOpenDatetime >= @WorkdayOpenDatetimeGte)
            AND (@WorkdayOpenDatetimeLte IS NULL OR a.WorkdayOpenDatetime < @WorkdayOpenDatetimeLte)
            ORDER BY a.WorkdayOpenDatetime DESC
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY
            ";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            WorkdayOpenDatetimeGte = workdayOpenDatetimeGte,
            WorkdayOpenDatetimeLte = workdayOpenDatetimeLte,
            Offset = offset,
            PageSize = pageSize
        };

        if (db != null)
        {
            return await (db.QueryAsync<ReportTurnoverHeader>(query, parameters)).ConfigureAwait(false);
        }

        return [];
    }
}