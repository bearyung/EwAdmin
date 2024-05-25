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
        HashSet<string> includedProperties)
    {
        var updateQuery = new StringBuilder("UPDATE ItemCategory SET ");
        var parameters = new DynamicParameters();

        var properties = itemCategoryObj.GetType().GetProperties();
        var setClauses = new List<string>();

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null
            && httpContext.Items.TryGetValue("NormalizedJsonProperties", out var normalizedPropertiesObj)
            && normalizedPropertiesObj is Dictionary<string, object?> normalizedProperties)
        {
            foreach (var property in properties)
            {
                if (!includedProperties.Contains(property.Name)) continue;

                var jsonPropertyName = JsonNamingPolicy.CamelCase.ConvertName(property.Name).ToLower();

                if (!normalizedProperties.ContainsKey(jsonPropertyName)) continue;
                var value = property.GetValue(itemCategoryObj);
                setClauses.Add($"{property.Name} = @{property.Name}");
                parameters.Add($"@{property.Name}", value);
            }
        }

        updateQuery.Append(string.Join(", ", setClauses));
        updateQuery.Append(" WHERE AccountId = @AccountId AND CategoryId = @CategoryId");
        parameters.Add("@AccountId", itemCategoryObj.AccountId);
        parameters.Add("@CategoryId", itemCategoryObj.CategoryId);

        return (updateQuery.ToString(), parameters);
    }
}