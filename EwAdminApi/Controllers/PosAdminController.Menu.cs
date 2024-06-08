using System.ComponentModel.DataAnnotations;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

public partial class PosAdminController
{
    /// <summary>
    /// Handles the GET request to fetch the list of item categories.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="showEnabledRecords"></param>
    /// <param name="showDisabledRecords"></param>
    /// <param name="lastModifiedDateTime"></param>
    /// <param name="categoryNameContains"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the item categories are found, it returns an HTTP 200 status code along with the item category details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// - If the pageSize is more than 100, it also returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("itemCategoryList")]
    [ProducesResponseType(typeof(IEnumerable<ItemCategory>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetItemCategoryList(
        [FromQuery, Required] int accountId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] bool showEnabledRecords = true, [FromQuery] bool showDisabledRecords = false,
        [FromQuery] DateTime? lastModifiedDateTime = null, [FromQuery] string? categoryNameContains = null)
    {
        // Check if page or pageSize is less than or equal to 0. If so, return a BadRequest.
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        // Check if pageSize is more than 100. If so, return a BadRequest.
        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }

        // get the item category list from PosItemCategoryRepository
        var resultList = await _posItemCategoryRepository
            .GetItemCategoryListAsync(accountId: accountId, page, pageSize,
                showEnabledRecords, showDisabledRecords,
                lastModifiedDateTime, categoryNameContains)
            .ConfigureAwait(false);

        return Ok(resultList);
    }

    /// <summary>
    /// Handles the GET request to fetch the details of a specific item category.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="categoryId"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the item category is found, it returns an HTTP 200 status code along with the item category details.
    /// - If the item category is not found, it returns an HTTP 404 status code with a custom error message.
    /// </returns>
    [HttpGet("itemCategory")]
    [ProducesResponseType(typeof(ItemCategory), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 404)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetItemCategory(
        [FromQuery, Required] int accountId, [FromQuery, Required] int categoryId)
    {
        // Fetches the item category details using the provided account ID and item category ID.
        var itemCategory = await _posItemCategoryRepository.GetItemCategoryDetailAsync(accountId, categoryId)
            .ConfigureAwait(false);

        // If the item category is not found, returns a custom 404 Not Found response with a custom error message.
        if (itemCategory == null)
        {
            return new CustomNotFoundRequestResult("Item category not found.");
        }

        // If the item category is found, returns an HTTP 200 OK response with the item category details.
        return Ok(itemCategory);
    }

    /// <summary>
    /// Handles the PATCH request to update an item category.
    /// </summary>
    /// <param name="itemCategory"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the item category is updated successfully, it returns an HTTP 200 status code along with the updated item category details.
    /// </returns>
    [HttpPatch("updateItemCategory")]
    [ProducesResponseType(typeof(ItemCategory), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateItemCategory(
        [JsonBinder, Required] ItemCategory itemCategory)
    {
        // Validate the ID fields
        if (itemCategory.AccountId == 0 || itemCategory.CategoryId == 0)
        {
            return new CustomBadRequestResult("AccountId and ItemCategoryId must be greater than zero.");
        }

        // Get the Monday user data from the HttpContext
        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;

        if (mondayUserData == null)
        {
            // Handle the case where the data is not found
            return new CustomBadRequestResult("User data not found.");
        }

        // override the modified by field with the username from Monday
        itemCategory.ModifiedBy = mondayUserData.Data?.Me?.Name;
        itemCategory.ModifiedDate = DateTime.Now;

        // Implementation to update item category
        var updatedItemCategory =
            await _posItemCategoryRepository.UpdateItemCategoryAsync(itemCategory).ConfigureAwait(false);

        // If the item category is not updated successfully, return a custom 400 Bad Request response with a custom error message.
        // otherwise, return an HTTP 200 OK response with the updated item category.
        if (updatedItemCategory == null)
        {
            return new CustomBadRequestResult("Failed to update item category.");
        }

        return Ok(updatedItemCategory);
    }
}