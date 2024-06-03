using System.Text;
using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosTxSalesRepositoryBase(
    IConnectionService connectionService,
    IHttpContextAccessor httpContextAccessor)
    : PosRepositoryBase(connectionService, httpContextAccessor)
{
    protected (string query, DynamicParameters parameters) BuildTxSalesHeaderUpdateQuery(TxSalesHeader inputObj,
        HashSet<string> includedProperties, HashSet<string>? explicitProperties = null)
    {
        ArgumentNullException.ThrowIfNull(inputObj);
        var updateQuery = new StringBuilder("UPDATE TxSalesHeader SET ");
        
        var (setClauses, parameters) = BuildUpdateQuerySetClause(inputObj, includedProperties, explicitProperties);

        updateQuery.Append(string.Join(", ", setClauses));
        updateQuery.Append(" WHERE AccountId = @AccountId AND ShopId = @ShopId AND TxSalesHeaderId = @TxSalesHeaderId");
        parameters.Add("@AccountId", inputObj.AccountId);
        parameters.Add("@ShopId", inputObj.ShopId);
        parameters.Add("@TxSalesHeaderId", inputObj.TxSalesHeaderId);

        return (updateQuery.ToString(), parameters);
    }
}