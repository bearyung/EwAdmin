using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosItemCategoryRepository : PosItemCategoryRepositoryBase
{
    public PosItemCategoryRepository(IConnectionService connectionService, IHttpContextAccessor httpContextAccessor) :
        base(connectionService, httpContextAccessor)
    {
    }

    // input: accountId, shopId, page, pageSize, Enabled (1 = true, 0 = false), lastModifiedDate
    // output: list of ItemCategory
    // a method to get the list of item category with pagination
    public async Task<IEnumerable<ItemCategory>> GetItemCategoryListAsync
    (int accountId, int page, int pageSize, bool showEnabledRecords = true, bool showDisabledRecords = false,
        DateTime? lastModifiedDateTime = null, string? categoryNameContains = null)
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
            AND (@CategoryNameContains IS NULL OR CategoryName LIKE N'%' + @CategoryNameContains + '%')
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
            LastModifiedDateTime = lastModifiedDateTime,
            CategoryNameContains = categoryNameContains
        };

        if (db != null)
        {
            return await (db.QueryAsync<ItemCategory>(query, parameters)).ConfigureAwait(false);
        }

        return [];
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
    /// <param name="itemCategoryObj"></param>
    /// <returns>
    /// Returns true if the item category is updated successfully
    /// </returns>
    public async Task<ItemCategory?> UpdateItemCategoryAsync(ItemCategory itemCategoryObj)
    {
        using var db = await GetPosDatabaseConnectionByAccount(itemCategoryObj.AccountId).ConfigureAwait(false);
        var (query, parameters) = BuildItemCategoryUpdateQuery(itemCategoryObj, [
            nameof(ItemCategory.ParentCategoryId),
            nameof(ItemCategory.IsTerminal),
            nameof(ItemCategory.Enabled),
            nameof(ItemCategory.CategoryTypeId)
        ], [
            nameof(ItemCategory.ModifiedDate),
            nameof(ItemCategory.ModifiedBy),
        ]);

        if (db != null)
        {
            // if update is successful, return the updated item category
            var result = await db.ExecuteAsync(query, parameters).ConfigureAwait(false);
            if (result > 0)
            {
                return await GetItemCategoryDetailAsync(itemCategoryObj.AccountId, itemCategoryObj.CategoryId)
                    .ConfigureAwait(false);
            }
        }

        return null;
    }
    
    public async Task<ItemCategory?> InsertItemCategoryAsync(ItemCategory itemCategoryObj)
    {
        using var db = await GetPosDatabaseConnectionByAccount(itemCategoryObj.AccountId).ConfigureAwait(false);
        
        // create the new item category
        // return the newly created item category (with the new CategoryId)
        // code here
        var query = @"
            INSERT INTO [dbo].[ItemCategory]
            (
                 [AccountId]
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
            )
            VALUES
            (
                 @AccountId
                ,@CategoryName
                ,@CategoryNameAlt
                ,@DisplayIndex
                ,@ParentCategoryId
                ,@IsTerminal
                ,@IsPublicDisplay
                ,@ButtonStyleId
                ,@PrinterName
                ,@IsModifier
                ,@Enabled
                ,@CreatedDate
                ,@CreatedBy
                ,@ModifiedDate
                ,@ModifiedBy
                ,@PrinterName2
                ,@PrinterName3
                ,@PrinterName4
                ,@PrinterName5
                ,@CategoryTypeId
                ,@ImageFileName
                ,@ImageFileName2
                ,@ImageFileName3
                ,@IsSelfOrderingDisplay
                ,@IsOnlineStoreDisplay
                ,@CategoryCode
            )
            SELECT SCOPE_IDENTITY()
            ";
        
        var parameters = new
        {
            itemCategoryObj.AccountId,
            itemCategoryObj.CategoryName,
            itemCategoryObj.CategoryNameAlt,
            itemCategoryObj.DisplayIndex,
            itemCategoryObj.ParentCategoryId,
            itemCategoryObj.IsTerminal,
            itemCategoryObj.IsPublicDisplay,
            itemCategoryObj.ButtonStyleId,
            itemCategoryObj.PrinterName,
            itemCategoryObj.IsModifier,
            itemCategoryObj.Enabled,
            itemCategoryObj.CreatedDate,
            itemCategoryObj.CreatedBy,
            itemCategoryObj.ModifiedDate,
            itemCategoryObj.ModifiedBy,
            itemCategoryObj.PrinterName2,
            itemCategoryObj.PrinterName3,
            itemCategoryObj.PrinterName4,
            itemCategoryObj.PrinterName5,
            itemCategoryObj.CategoryTypeId,
            itemCategoryObj.ImageFileName,
            itemCategoryObj.ImageFileName2,
            itemCategoryObj.ImageFileName3,
            itemCategoryObj.IsSelfOrderingDisplay,
            itemCategoryObj.IsOnlineStoreDisplay,
            itemCategoryObj.CategoryCode
        };
        
        if (db != null)
        {
            var newCategoryId = await db.QueryFirstOrDefaultAsync<int>(query, parameters).ConfigureAwait(false);
            if (newCategoryId > 0)
            {
                return await GetItemCategoryDetailAsync(itemCategoryObj.AccountId, newCategoryId).ConfigureAwait(false);
            }
        }
        return null;
    }
    
    public async Task<ItemCategoryShopDetail?> InsertItemCategoryShopDetailAsync(ItemCategoryShopDetail itemCategoryShopDetailObj)
    {
        using var db = await GetPosDatabaseConnectionByAccount(itemCategoryShopDetailObj.AccountId).ConfigureAwait(false);
        
        // create the new item category shop detail
        // return the newly created item category shop detail (with the new CategoryId)
        // code here
        var query = @"
            INSERT INTO [dbo].[ItemCategoryShopDetail]
            (
                 [CategoryId]
                ,[ShopId]
                ,[AccountId]
                ,[DisplayName]
                ,[DisplayIndex]
                ,[IsPublicDisplay]
                ,[Enabled]
                ,[CreatedDate]
                ,[CreatedBy]
                ,[ModifiedDate]
                ,[ModifiedBy]
            )
            VALUES
            (
                 @CategoryId
                ,@ShopId
                ,@AccountId
                ,@DisplayName
                ,@DisplayIndex
                ,@IsPublicDisplay
                ,@Enabled
                ,@CreatedDate
                ,@CreatedBy
                ,@ModifiedDate
                ,@ModifiedBy
            )
            SELECT SCOPE_IDENTITY()
            ";
        
        var parameters = new
        {
            itemCategoryShopDetailObj.CategoryId,
            itemCategoryShopDetailObj.ShopId,
            itemCategoryShopDetailObj.AccountId,
            itemCategoryShopDetailObj.DisplayName,
            itemCategoryShopDetailObj.DisplayIndex,
            itemCategoryShopDetailObj.IsPublicDisplay,
            itemCategoryShopDetailObj.Enabled,
            itemCategoryShopDetailObj.CreatedDate,
            itemCategoryShopDetailObj.CreatedBy,
            itemCategoryShopDetailObj.ModifiedDate,
            itemCategoryShopDetailObj.ModifiedBy
        };
        
        if (db != null)
        {
            var newCategoryShopDetailId = await db.QueryFirstOrDefaultAsync<int>(query, parameters).ConfigureAwait(false);
            if (newCategoryShopDetailId > 0)
            {
                return await GetItemCategoryShopDetailAsync(
                    itemCategoryShopDetailObj.AccountId, 
                    itemCategoryShopDetailObj.ShopId, 
                    itemCategoryShopDetailObj.CategoryId)
                    .ConfigureAwait(false);
            }
        }
        return null;
    }
    
    public async Task<ItemCategoryShopDetail?> GetItemCategoryShopDetailAsync(
        int accountId, 
        int shopId,
        int categoryId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT 
                ,[CategoryId]
                ,[ShopId]
                ,[AccountId]
                ,[DisplayName]
                ,[DisplayIndex]
                ,[IsPublicDisplay]
                ,[Enabled]
                ,[CreatedDate]
                ,[CreatedBy]
                ,[ModifiedDate]
                ,[ModifiedBy]
            FROM [dbo].[ItemCategoryShopDetail]
            WHERE AccountId = @AccountId
            AND ShopId = @ShopId
            AND CategoryShopDetailId = @CategoryShopDetailId
            ";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            CategoryId = categoryId
        };

        if (db != null)
        {
            return await db.QueryFirstOrDefaultAsync<ItemCategoryShopDetail>(query, parameters).ConfigureAwait(false);
        }

        return null;
    }
}