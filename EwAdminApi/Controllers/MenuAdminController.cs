using EwAdmin.Common.Models.Pos;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuAdminController : ControllerBase
{
    private readonly PosItemCategoryRepository _posItemCategoryRepository;

    public MenuAdminController(PosItemCategoryRepository posItemCategoryRepository)
    {
        _posItemCategoryRepository = posItemCategoryRepository;
    }

    /// <summary>
    /// Handles the POST request to create item categories in a batch for a Point of Sale (POS) system.
    /// </summary>
    /// <param name="accountId">The account ID for which the item categories are being created.</param>
    /// <param name="shopIdList">The list of shop IDs for which the item categories are relevant. This parameter is optional and can be null.</param>
    /// <param name="itemCategoryNameList">The list of item category names to be created. 
    /// - Each category name may represent a top-level or subcategory:
    ///   - Names starting with "-" denote subcategories.
    ///   - Names starting with "--", "---", "----", or "-----" represent subcategories at respective levels up to 5.
    ///   - Unstructured names or those with unnecessary spaces will be trimmed and properly handled.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the item categories are created successfully, it returns an HTTP 200 status code with the list of created item categories.
    /// - If the itemCategoryNameList is empty or accountId is 0, it returns an HTTP 400 status code with a custom error message.
    /// - If any category exceeds the maximum allowed subcategory level (5), it returns an HTTP 400 status code with a custom error message.
    /// - If the Monday user data is not found in the context, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpPost("createItemCategories")]
    [ProducesResponseType(typeof(IEnumerable<ItemCategory>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateItemCategories(
        [FromBody] int accountId,
        [FromBody] List<int>? shopIdList,
        [FromBody] List<string> itemCategoryNameList)
    {
        if (itemCategoryNameList.Count == 0 || accountId == 0)
        {
            return new CustomBadRequestResult("Item category name list or accountId cannot be empty.");
        }

        if (itemCategoryNameList.Any(x => x.StartsWith("------")))
        {
            return new CustomBadRequestResult("Sub category level cannot be more than 5.");
        }

        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;
        if (mondayUserData == null)
        {
            return new CustomBadRequestResult("User data not found.");
        }

        var itemCategoryList = new List<ItemCategory>();
        var categoryPathDictionary = new Dictionary<string, ItemCategory>();
        int displayIndex = 1;

        foreach (var itemCategoryName in itemCategoryNameList)
        {
            // Trim spaces from the category name
            string trimmedName = itemCategoryName.Trim();

            // Count leading dashes to determine hierarchy level
            int dashCount = trimmedName.TakeWhile(c => c == '-').Count();

            // Determine parent path based on the hierarchy level
            string parentPath = string.Join(" - ", trimmedName.Split(' ').Take(dashCount).ToArray()).Trim();

            // Extract actual category name after removing leading dashes and trimming
            string actualCategoryName = trimmedName.Substring(dashCount).Trim();

            var itemCategoryObj = new ItemCategory
            {
                AccountId = accountId,
                CategoryName = actualCategoryName,
                IsPublicDisplay = true,
                DisplayIndex = displayIndex,
                Enabled = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = mondayUserData?.Data?.Me?.Name,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = mondayUserData?.Data?.Me?.Name,
            };

            // Assign parent category ID if it's a subcategory
            if (dashCount > 0 && categoryPathDictionary.ContainsKey(parentPath))
            {
                itemCategoryObj.ParentCategoryId = categoryPathDictionary[parentPath].CategoryId;
            }

            // Save item category to the database
            var result = await _posItemCategoryRepository.InsertItemCategoryAsync(itemCategoryObj)
                .ConfigureAwait(false);
            if (result == null) continue;
            
            // Insert the shop category mapping if shop IDs are provided
            if (shopIdList is { Count: > 0 })
            {
                foreach (var shopId in shopIdList)
                {
                    await _posItemCategoryRepository.InsertItemCategoryShopDetailAsync(
                        new ItemCategoryShopDetail
                        {
                            CategoryId = result.CategoryId,
                            ShopId = shopId,
                            AccountId = accountId,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = mondayUserData?.Data?.Me?.Name,
                            ModifiedDate = DateTime.UtcNow,
                            ModifiedBy = mondayUserData?.Data?.Me?.Name,
                            Enabled = true,
                            IsPublicDisplay = true
                        }).ConfigureAwait(false);
                }
            }
                
            // Update the dictionary with the current category's path
            string currentPath = string.Join(" - ", trimmedName.Split(' ').Take(dashCount + 1).ToArray()).Trim();
            categoryPathDictionary[currentPath] = result;
            itemCategoryList.Add(result);
            displayIndex++;
        }

        return Ok(itemCategoryList);
    }
}
