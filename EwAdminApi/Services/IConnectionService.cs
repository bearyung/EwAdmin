using System.Data;

namespace EwAdminApi.Services;

public interface IConnectionService
{
    IDbConnection? GetConnection();
    IDbConnection? GetConnection(string dbServer, string dbName, string dbUsername, string dbPassword);
}