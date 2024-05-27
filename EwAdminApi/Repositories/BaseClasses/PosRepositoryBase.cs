using System.Data;
using System.Text;
using System.Text.Json;
using Dapper;
using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories.BaseClasses;

public class PosRepositoryBase(IConnectionService connectionService, IHttpContextAccessor? httpContextAccessor = null)
{
    private readonly IConnectionService _connectionService = connectionService;
    private readonly IHttpContextAccessor? _httpContextAccessor = httpContextAccessor;

    protected async Task<IDbConnection?> GetPosDatabaseConnectionByAccount(int brandId)
    {
        using var db = _connectionService.GetConnection();
        var query = @"
            select region.DBServer, region.DBName, region.DBUsername, region.DBPassword
            from webadmin_BrandMaster brand, webadmin_CompanyMaster company, webadmin_ResellerAreaMaster reseller, webadmin_RegionMaster region
            WHERE brand.CompanyId = company.CompanyId
            and company.ResellerAreaId = reseller.ResellerAreaId
            and reseller.RegionId = region.RegionId
            and brandId = @BrandId";

        var parameters = new
        {
            BrandId = brandId
        };

        if (db != null)
        {
            var result = await db.QuerySingleOrDefaultAsync<RegionMasterDatabaseMetadata>(query, parameters)
                .ConfigureAwait(false);

            if (result is { DbServer: not null, DbName: not null, DbUsername: not null, DbPassword: not null })
                return _connectionService.GetConnection(result.DbServer, result.DbName,
                    result.DbUsername,
                    result.DbPassword);
        }

        return null;
    }

    protected async Task<IDbConnection?> GetPosDatabaseConnectionByRegion(int regionId)
    {
        using var db = _connectionService.GetConnection();
        var query = @"
            select region.DBServer, region.DBName, region.DBUsername, region.DBPassword
            from webadmin_RegionMaster region
            WHERE regionId = @RegionId";

        var parameters = new
        {
            RegionId = regionId
        };

        if (db != null)
        {
            var result = await db.QuerySingleOrDefaultAsync<RegionMasterDatabaseMetadata>(query, parameters)
                .ConfigureAwait(false);

            if (result is { DbServer: not null, DbName: not null, DbUsername: not null, DbPassword: not null })
                return _connectionService.GetConnection(result.DbServer, result.DbName,
                    result.DbUsername,
                    result.DbPassword);
        }

        return null;
    }

    /// <summary>
    /// Method for building the update query's set clause
    /// </summary>
    /// <param name="updateObj">
    /// The object containing the properties to be updated
    /// </param>
    /// <param name="includedProperties">
    /// The properties that are included in the normalized JSON properties received from the client
    /// </param>
    /// <param name="explicitProperties">
    /// The properties that are explicitly included in the update query
    /// </param>
    /// <returns>
    /// A tuple containing the set clauses and the parameters
    /// - set clauses example: Property1 = @Property1, Property2 = @Property2
    /// - parameters example: { @Property1 = value1, @Property2 = value2 }
    /// </returns>
    protected (List<string> setClauses, DynamicParameters parameters) BuildUpdateQuerySetClause(
        object updateObj,
        HashSet<string> includedProperties, 
        HashSet<string>? explicitProperties)
    {
        var properties = updateObj.GetType().GetProperties();
        var parameters = new DynamicParameters();
        var setClauses = new List<string>();

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null
            || !httpContext.Items.TryGetValue("NormalizedJsonProperties", out var normalizedPropertiesObj)
            || normalizedPropertiesObj is not Dictionary<string, object?> normalizedProperties)
            return (setClauses, parameters);
        foreach (var property in properties)
        {
            if (includedProperties.Contains(property.Name))
            {
                var jsonPropertyName = JsonNamingPolicy.CamelCase.ConvertName(property.Name).ToLower();

                if (!normalizedProperties.ContainsKey(jsonPropertyName)) continue;
                var value = property.GetValue(updateObj);
                setClauses.Add($"{property.Name} = @{property.Name}");
                parameters.Add($"@{property.Name}", value);
            }
            else if (explicitProperties != null && explicitProperties.Contains(property.Name))
            {
                var value = property.GetValue(updateObj);
                setClauses.Add($"{property.Name} = @{property.Name}");
                parameters.Add($"@{property.Name}", value);
            }
        }

        return (setClauses, parameters);
    }
}