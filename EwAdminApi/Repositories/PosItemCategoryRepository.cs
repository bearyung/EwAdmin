using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosItemCategoryRepository : PosRepositoryBase
{
    public PosItemCategoryRepository(IConnectionService connectionService) : base(connectionService)
    {
    }

    // input: accountId, shopId, page, pageSize, Enabled (1 = true, 0 = false), lastModifiedDate
    // output: list of ItemCategory
    // a method to get the list of item category with pagination
    public async Task<IEnumerable<ItemCategory>?> GetItemCategoryListAsync
    (int accountId, int page, int pageSize, bool showEnabledRecords = true, bool showDisabledRecords = false,
        DateTime? lastModifiedDateTime = null)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;
        var query = @"
            SELECT 
                 [CategoryId]
                ,[AccountId]
                ,[CategoryName]
                ,[CategoryNameAlt]
                ,[DisplayIndex]
                ,[ParentCategoryId]
                ,[IsTerminal]
                ,[IsPublicDisplay]
                ,[ButtonStyleId]
                ,[PrinterName]
                ,[IsModifier]
                ,[Enabled]
                ,[CreatedDate]
                ,[CreatedBy]
                ,[ModifiedDate]
                ,[ModifiedBy]
                ,[PrinterName2]
                ,[PrinterName3]
                ,[PrinterName4]
                ,[PrinterName5]
                ,[CategoryTypeId]
                ,[ImageFileName]
                ,[ImageFileName2]
                ,[ImageFileName3]
                ,[IsSelfOrderingDisplay]
                ,[IsOnlineStoreDisplay]
                ,[CategoryCode]
            FROM [dbo].[ItemCategory]
            WHERE AccountId = @AccountId
            AND (@ShowEnabledRecords = 1 OR Enabled = 0)
            AND (@ShowDisabledRecords = 1 OR Enabled = 1)
            AND (@LastModifiedDateTime IS NULL OR ModifiedDate >= @LastModifiedDateTime)
            ORDER BY DisplayIndex
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
            ";

        var parameters = new
        {
            AccountId = accountId,
            Offset = offset,
            PageSize = pageSize,
            ShowEnabledRecords = showEnabledRecords ? 1 : 0,
            ShowDisabledRecords = showDisabledRecords ? 1 : 0,
            LastModifiedDateTime = lastModifiedDateTime
        };

        if (db != null)
        {
            return await (db.QueryAsync<ItemCategory>(query, parameters)).ConfigureAwait(false);
        }

        return null;
    }

    // input: accountId, categoryId
    // output: ItemCategory object
    // a method to get the item category detail
    public async Task<ItemCategory?> GetItemCategoryDetailAsync(int accountId, int categoryId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT 
                 [CategoryId]
                ,[AccountId]
                ,[CategoryName]
                ,[CategoryNameAlt]
                ,[DisplayIndex]
                ,[ParentCategoryId]
                ,[IsTerminal]
                ,[IsPublicDisplay]
                ,[ButtonStyleId]
                ,[PrinterName]
                ,[IsModifier]
                ,[Enabled]
                ,[CreatedDate]
                ,[CreatedBy]
                ,[ModifiedDate]
                ,[ModifiedBy]
                ,[PrinterName2]
                ,[PrinterName3]
                ,[PrinterName4]
                ,[PrinterName5]
                ,[CategoryTypeId]
                ,[ImageFileName]
                ,[ImageFileName2]
                ,[ImageFileName3]
                ,[IsSelfOrderingDisplay]
                ,[IsOnlineStoreDisplay]
                ,[CategoryCode]
            FROM [dbo].[ItemCategory]
            WHERE AccountId = @AccountId
            AND CategoryId = @CategoryId
            ";

        var parameters = new
        {
            AccountId = accountId,
            CategoryId = categoryId
        };

        if (db != null)
        {
            return await db.QueryFirstOrDefaultAsync<ItemCategory>(query, parameters).ConfigureAwait(false);
        }

        return null;
    }


    // a method to update the item category
    // input: ItemCategory object
    //          only the following fields are updated:
    //          ParentCategoryId, IsTerminal, Enabled
    // output: boolean
    /// <summary>
    /// Update an item category
    /// </summary>
    /// <param name="itemCategory"></param>
    /// <returns>
    /// Returns true if the item category is updated successfully
    /// </returns>
    public async Task<ItemCategory?> UpdateItemCategoryAsync(ItemCategory itemCategory)
    {
        using var db = await GetPosDatabaseConnectionByAccount(itemCategory.AccountId).ConfigureAwait(false);
        var query = @"
            UPDATE [dbo].[ItemCategory]
            SET 
                ParentCategoryId = @ParentCategoryId,
                IsTerminal = @IsTerminal,
                Enabled = @Enabled, 
                ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
            WHERE CategoryId = @CategoryId
            AND AccountId = @AccountId
            ";

        var parameters = new
        {
            itemCategory.ParentCategoryId,
            itemCategory.IsTerminal,
            itemCategory.Enabled,
            itemCategory.ModifiedBy,
            itemCategory.CategoryId,
            itemCategory.AccountId
        };

        if (db != null)
        {
            // if updqte is successful, return the updated item category
            var result = await db.ExecuteAsync(query, parameters).ConfigureAwait(false);
            if (result > 0)
            {
                return await GetItemCategoryDetailAsync(itemCategory.AccountId, itemCategory.CategoryId)
                    .ConfigureAwait(false);
            }
        }

        return null;
    }
}