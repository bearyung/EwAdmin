using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosTableMasterRepository(
    IConnectionService connectionService,
    IHttpContextAccessor? httpContextAccessor = null) : PosRepositoryBase(connectionService, httpContextAccessor)
{
    // add a new method to get the table list with pagination
    public async Task<IEnumerable<TableMaster>> GetTableListAsync(int accountId, int shopId, int page, int pageSize,
        int? tableId = null, string? tableCode = null, 
        bool showDisabled = false, bool showEnabled = true, bool showTempTable = true, bool showTakeAway = true, bool showDineIn = true)
    {
        
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        
        var sql = @"
            SELECT
                tm.TableId,
                tm.AccountId,
                tm.ShopId,
                tm.TableCode,
                tm.SectionId,
                tm.Description,
                tm.DescriptionAlt,
                tm.TableTypeId,
                tm.TableStatusId,
                tm.IsTakeAway,
                tm.IsTempTable,
                tm.Enabled,
                tm.CreatedDate,
                tm.CreatedBy,
                tm.ModifiedDate,
                tm.ModifiedBy,
                tm.DisplayIndex,
                tm.ParentTableId,
                tm.IsAppearOnFloorPlan,
                tm.SeatNum,
                ts.SectionName
            FROM TableMaster tm
            LEFT JOIN TableSection ts ON
                tm.accountId = ts.accountId AND
                tm.SectionId = ts.SectionId
            WHERE
                tm.AccountId = @AccountId AND
                tm.ShopId = @ShopId AND
                (@TableId IS NULL OR tm.TableId = @TableId) AND
                (@TableCode IS NULL OR tm.TableCode = @TableCode) AND
                (@ShowDisabled = 1 OR tm.Enabled = 1) AND
                (@ShowEnabled = 1 OR tm.Enabled = 0) AND
                (@ShowTempTable = 1 OR tm.IsTempTable = 0) AND
                (@ShowTakeAway = 1 OR tm.IsTakeAway = 0) AND
                (@ShowDineIn = 1 OR tm.IsTakeAway = 1)
            ORDER BY tm.TableId
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var offset = (page - 1) * pageSize;

        var parameters = new
        {
            TableId = tableId,
            TableCode = tableCode,
            ShowDisabled = showDisabled ? 1 : 0,
            ShowEnabled = showEnabled ? 1 : 0,
            ShowTempTable = showTempTable ? 1 : 0,
            ShowTakeAway = showTakeAway ? 1 : 0,
            ShowDineIn = showDineIn? 1 : 0, 
            Offset = offset,
            PageSize = pageSize,
            AccountId = accountId,
            ShopId = shopId
        };
        
        if (db != null)
        {
            return await db.QueryAsync<TableMaster>(sql, parameters).ConfigureAwait(false);
        }
        
        return [];
    }
}