using System.ComponentModel.DataAnnotations;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using EwAdminApi.Models.Pos;
using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PosAdminController : ControllerBase
{
    private readonly PosShopRepository _posShopRepository;
    private readonly PosShopWorkdayDetailRepository _posShopWorkdayDetailRepository;
    private readonly PosShopWorkdayPeriodDetailRepository _posShopWorkdayPeriodDetailRepository;

    public PosAdminController(
        PosShopRepository posShopRepository,
        PosShopWorkdayDetailRepository posShopWorkdayDetailRepository,
        PosShopWorkdayPeriodDetailRepository posShopWorkdayPeriodDetailRepository)
    {
        _posShopRepository = posShopRepository;
        _posShopWorkdayDetailRepository = posShopWorkdayDetailRepository;
        _posShopWorkdayPeriodDetailRepository = posShopWorkdayPeriodDetailRepository;
    }
    
    /// <summary>
    /// Handles the GET request to fetch the details of a specific shop.
    /// </summary>
    /// <param name="accountId">The account ID from the query string.</param>
    /// <param name="shopId">The shop ID from the query string.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop is found, it returns an HTTP 200 status code along with the shop details.
    /// - If the shop is not found, it returns an HTTP 404 status code with a custom error message.
    /// </returns>
    [HttpGet("shop")]
    [ProducesResponseType(typeof(Shop), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 404)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetShopDetail(
        [FromQuery] int accountId, [FromQuery] int shopId)
    {
        // Fetches the shop details using the provided account ID and shop ID.
        var shop = await _posShopRepository.GetShopDetailAsync(accountId, shopId);

        // If the shop is not found, returns a custom 404 Not Found response with a custom error message.
        if (shop == null)
        {
            return new CustomNotFoundRequestResult("Shop not found.");
        }

        // If the shop is found, returns an HTTP 200 OK response with the shop details.
        return Ok(shop);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the list of shop workday details.
    /// </summary>
    /// <param name="accountId">The account ID from the query string.</param>
    /// <param name="shopId">The shop ID from the query string.</param>
    /// <param name="startDate">The start date for filtering the workday details. If not provided, fetches the latest 10 records.</param>
    /// <param name="endDate">The end date for filtering the workday details.</param>
    /// <param name="page">The page number for pagination. Defaults to 1 if not provided.</param>
    /// <param name="pageSize">The number of records per page. Defaults to 20 if not provided.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop workday details are found, it returns an HTTP 200 status code along with the workday details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// - If the pageSize is more than 100, it also returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("shopworkdaydetaillist")]
    [ProducesResponseType(typeof(IEnumerable<ShopWorkdayDetail>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetShopWorkdayDetailList(
        [FromQuery] int accountId, [FromQuery] int shopId, 
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch shop workday detail
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
        
        // get the shop workday detail list from PosShopWorkdayDetailRepository
        var resultList = 
            await _posShopWorkdayDetailRepository.GetShopWorkdayDetailListAsync(accountId, shopId, startDate, endDate, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the list of shop workday period details.
    /// </summary>
    /// <param name="accountId">The account ID from the query string.</param>
    /// <param name="shopId">The shop ID from the query string.</param>
    /// <param name="workdayDetailId">The workday detail ID from the query string.</param>
    /// <param name="page">The page number for pagination. Defaults to 1 if not provided.</param>
    /// <param name="pageSize">The number of records per page. Defaults to 20 if not provided.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop workday period details are found, it returns an HTTP 200 status code along with the workday period details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// - If the pageSize is more than 100, it also returns an HTTP 400 status code with a custom error message.
    /// </returns>
    
    [HttpGet("shopworkdayperioddetaillist")]
    [ProducesResponseType(typeof(IEnumerable<ShopWorkdayPeriodDetail>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetShopWorkdayPeriodDetailList(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery, Required] int workdayDetailId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch shop workday period detail
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }
        
        // get the shop workday period detail list from PosShopWorkdayPeriodDetailRepository
        var resultList = 
            await _posShopWorkdayPeriodDetailRepository.GetShopWorkdayPeriodDetailListAsync(accountId, shopId, workdayDetailId, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
    
    /// <summary>
    /// Handles the POST request to update a shop workday period detail.
    /// </summary>
    /// <param name="shopWorkdayPeriodDetail">The shop workday period detail to be updated. Only startdatetime, enddatetime, and enabled fields are updated.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop workday period detail is updated successfully, it returns an HTTP 200 status code along with the updated shop workday period detail.
    /// - If the user data is not found, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
        
    [HttpPatch("updateShopWorkdayPeriodDetail")]
    [ProducesResponseType(typeof(ShopWorkdayPeriodDetail), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateShopWorkdayPeriodDetail(
        [FromBody] ShopWorkdayPeriodDetail shopWorkdayPeriodDetail)
    {
        // Validate the identify fields
        if (shopWorkdayPeriodDetail.AccountId == 0 || shopWorkdayPeriodDetail.ShopId == 0 || shopWorkdayPeriodDetail.WorkdayPeriodDetailId == 0)
        {
            return new CustomBadRequestResult("AccountId, ShopId and ShopWorkdayPeriodDetailId must be greater than zero.");
        }

        // Get the Monday user data from the HttpContext
        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;

        if (mondayUserData == null)
        {
            // Handle the case where the data is not found
            return new CustomBadRequestResult("User data not found.");
        }
        
        // override the modified by field with the user name from Monday
        shopWorkdayPeriodDetail.ModifiedBy = mondayUserData.Data.Me.Name;
        
        // Implementation to update shop workday period detail
        var updatedShopWorkdayPeriodDetail = 
            await _posShopWorkdayPeriodDetailRepository.UpdateShopWorkdayPeriodDetailAsync(shopWorkdayPeriodDetail).ConfigureAwait(false);
        
        return Ok(updatedShopWorkdayPeriodDetail);
    }
    
    /// <summary>
    /// Handles the POST request to update a shop workday detail.
    /// </summary>
    /// <param name="shopWorkdayDetail">The shop workday detail to be updated. Only opendatetime, closedatetime, isclosed, enabled, and modifiedby fields are updated.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop workday detail is updated successfully, it returns an HTTP 200 status code along with the updated shop workday detail.
    /// - If the user data is not found, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpPatch("updateShopWorkdayDetail")]
    [ProducesResponseType(typeof(ShopWorkdayDetail), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateShopWorkdayDetail(
        [FromBody] ShopWorkdayDetail shopWorkdayDetail)
    {
        // Validate the ID fields
        if (shopWorkdayDetail.AccountId == 0 || shopWorkdayDetail.ShopId == 0 || shopWorkdayDetail.WorkdayDetailId == 0)
        {
            return new CustomBadRequestResult("AccountId, ShopId and WorkdayDetailId must be greater than zero.");
        }

        // Get the Monday user data from the HttpContext
        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;

        if (mondayUserData == null)
        {
            // Handle the case where the data is not found
            return new CustomBadRequestResult("User data not found.");
        }
        
        // override the modified by field with the user name from Monday
        shopWorkdayDetail.ModifiedBy = mondayUserData.Data.Me.Name;
        
        // Implementation to update shop workday detail
        var updatedShopWorkdayDetail = 
            await _posShopWorkdayDetailRepository.UpdateShopWorkdayDetailAsync(shopWorkdayDetail).ConfigureAwait(false);
        
        return Ok(updatedShopWorkdayDetail);
    }
}