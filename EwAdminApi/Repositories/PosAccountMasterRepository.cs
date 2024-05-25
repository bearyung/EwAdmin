using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosAccountMasterRepository(IConnectionService connectionService) : PosRepositoryBase(connectionService)
{
    // add a new method to get the account master list with pagination
    // code here
    
    public async Task<IEnumerable<AccountMaster>> GetAccountMasterListAsync(int regionId, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByRegion(regionId).ConfigureAwait(false);
        
        // write the query to get the account master list with pagination
        var query = @"
            SELECT [AccountId]
                  ,[AccountName]
                  ,[AccountNameAlt]
                  ,[AccountCode]
                  ,[AccountType]
                  ,[ParentAccountId]
                  ,[IsTerminal]
                  ,[IsPublicDisplay]
                  ,[ButtonStyleId]
                  ,[PrinterName]
                  ,[Enabled]
                  ,[CreatedDate]
                  ,[CreatedBy]
                  ,[ModifiedDate]
                  ,[ModifiedBy]
                  ,[PrinterName2]
                  ,[PrinterName3]
                  ,[PrinterName4]
                  ,[PrinterName5]
                  ,[ImageFileName]
                  ,[ImageFileName2]
                  ,[ImageFileName3]
                  ,[IsSelfOrderingDisplay]
                  ,[IsOnlineStoreDisplay]
                  ,[AccountCode]
              FROM [dbo].[AccountMaster]
              WHERE [Enabled] = 1
              ORDER BY AccountId
              OFFSET @Offset ROWS
              FETCH NEXT @PageSize ROWS ONLY";
        
        // set the parameters
        var parameters = new
        {
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };
        
        // execute the query and return the result
        if (db != null)
        {
            return await db.QueryAsync<AccountMaster>(query, parameters).ConfigureAwait(false);    
        }

        return [];
    }
    
    // add a new method to get the account master by account id
    // code here
    public async Task<AccountMaster?> GetAccountMasterByIdAsync(int accountId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        
        // write the query to get the account master by account id
        var query = @"
            SELECT [AccountId]
                  ,[AccountName]
                  ,[AccountNameAlt]
                  ,[AccountCode]
                  ,[AccountType]
                  ,[ParentAccountId]
                  ,[IsTerminal]
                  ,[IsPublicDisplay]
                  ,[ButtonStyleId]
                  ,[PrinterName]
                  ,[Enabled]
                  ,[CreatedDate]
                  ,[CreatedBy]
                  ,[ModifiedDate]
                  ,[ModifiedBy]
                  ,[PrinterName2]
                  ,[PrinterName3]
                  ,[PrinterName4]
                  ,[PrinterName5]
                  ,[ImageFileName]
                  ,[ImageFileName2]
                  ,[ImageFileName3]
                  ,[IsSelfOrderingDisplay]
                  ,[IsOnlineStoreDisplay]
                  ,[AccountCode]
              FROM [dbo].[AccountMaster]
              WHERE [AccountId] = @AccountId";
        
        // set the parameters
        var parameters = new
        {
            AccountId = accountId
        };
        
        // execute the query and return the result
        if (db != null)
        {
            return await db.QueryFirstOrDefaultAsync<AccountMaster>(query, parameters).ConfigureAwait(false);    
        }

        return null;
    }
}