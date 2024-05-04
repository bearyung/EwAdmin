using System.Data;
using Microsoft.Data.SqlClient;

namespace EwAdminApi.Services;

public class ConnectionService : IConnectionService
{
    private readonly IConfiguration _configuration;

    public ConnectionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection? GetConnection()
    {
        // Get the connection sting
        var connectionString = _configuration
            .GetSection("ConnectionStrings:WebData")
            .Value;
        
        return new SqlConnection(connectionString);
    }

    public IDbConnection? GetConnection(
        string dbServer, string dbName, string dbUsername, string dbPassword)
    {
        string connectionString =
            $"Server=tcp:{dbServer},1433;" +
            $"Initial Catalog={dbName};Persist Security Info=False;" +
            $"User ID={dbUsername};" +
            $"Password={dbPassword};" +
            $"MultipleActiveResultSets=False;Encrypt=True;" +
            $"TrustServerCertificate=False;Connection Timeout=30;";
        return new SqlConnection(connectionString);
    }
}