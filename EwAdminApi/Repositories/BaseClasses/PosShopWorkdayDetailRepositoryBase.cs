using System.Text;
using System.Text.Json;
using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosShopWorkdayDetailRepositoryBase(
    IConnectionService connectionService,
    IHttpContextAccessor httpContextAccessor)
    : PosRepositoryBase(connectionService)
{
    protected (string query, DynamicParameters parameters) BuildUpdateQuery(ShopWorkdayDetail shopWorkdayDetailObj,
        HashSet<string> includedProperties, HashSet<string>? explicitProperties = null)
    {
        var updateQuery = new StringBuilder("UPDATE ShopWorkdayDetail SET ");
        var (setClauses, parameters) = BuildUpdateQuerySetClause(shopWorkdayDetailObj, includedProperties, explicitProperties);

        updateQuery.Append(string.Join(", ", setClauses));
        updateQuery.Append(@" 
            WHERE AccountId = @AccountId
            AND ShopId = @ShopId
            AND WorkdayDetailId = @WorkdayDetailId");
        parameters.Add("@AccountId", shopWorkdayDetailObj.AccountId);
        parameters.Add("@ShopId", shopWorkdayDetailObj.ShopId);
        parameters.Add("@WorkdayDetailId", shopWorkdayDetailObj.WorkdayDetailId);

        return (updateQuery.ToString(), parameters);
    }
}