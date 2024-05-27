using System.Text;
using System.Text.Json;
using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosItemCategoryRepositoryBase(
    IConnectionService connectionService,
    IHttpContextAccessor httpContextAccessor)
    : PosRepositoryBase(connectionService)
{
    protected (string query, DynamicParameters parameters) BuildUpdateQuery(ItemCategory itemCategoryObj,
        HashSet<string> includedProperties, HashSet<string>? explicitProperties = null)
    {
        var updateQuery = new StringBuilder("UPDATE ItemCategory SET ");
        
        var (setClauses, parameters) = BuildUpdateQuerySetClause(itemCategoryObj, includedProperties, explicitProperties);

        updateQuery.Append(string.Join(", ", setClauses));
        updateQuery.Append(" WHERE AccountId = @AccountId AND CategoryId = @CategoryId");
        parameters.Add("@AccountId", itemCategoryObj.AccountId);
        parameters.Add("@CategoryId", itemCategoryObj.CategoryId);

        return (updateQuery.ToString(), parameters);
    }
}