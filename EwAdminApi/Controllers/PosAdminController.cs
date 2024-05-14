using System.ComponentModel.DataAnnotations;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
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
    private readonly PosTxSalesRepository _posTxSalesRepository;
    private readonly PosPaymentMethodRepository _posPaymentMethodRepository;

    public PosAdminController(
        PosShopRepository posShopRepository,
        PosShopWorkdayDetailRepository posShopWorkdayDetailRepository,
        PosShopWorkdayPeriodDetailRepository posShopWorkdayPeriodDetailRepository,
        PosTxSalesRepository posTxSalesRepository,
        PosPaymentMethodRepository posPaymentMethodRepository)
    {
        _posShopRepository = posShopRepository;
        _posShopWorkdayDetailRepository = posShopWorkdayDetailRepository;
        _posShopWorkdayPeriodDetailRepository = posShopWorkdayPeriodDetailRepository;
        _posTxSalesRepository = posTxSalesRepository;
        _posPaymentMethodRepository = posPaymentMethodRepository;
    }
    
    // add an API endpoint shopList to return the list of shops
    // code here
    /// <summary>
    /// Handles the GET request to fetch the list of shops.
    /// </summary>
    /// <param name="accountId">The account ID from the query string.</param>
    /// <param name="shopId">The shop ID from the query string.</param>
    /// <param name="page">The page number for pagination. Defaults to 1 if not provided.</param>
    /// <param name="pageSize">The number of records per page. Defaults to 20 if not provided.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shops are found, it returns an HTTP 200 status code along with the shop details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// - If the pageSize is more than 100, it also returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("shopList")]
    [ProducesResponseType(typeof(IEnumerable<Shop>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetShopList(
        [FromQuery, Required] int accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? shopId = null)
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
        
        // get the shop list from PosShopRepository
        var resultList = await _posShopRepository.GetShopListAsync(accountId, page, pageSize, shopId).ConfigureAwait(false);
        
        return Ok(resultList);
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
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId)
    {
        // Fetches the shop details using the provided account ID and shop ID.
        var shop = await _posShopRepository.GetShopDetailAsync(accountId, shopId).ConfigureAwait(false);

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
    [HttpGet("shopWorkdayDetailList")]
    [ProducesResponseType(typeof(IEnumerable<ShopWorkdayDetail>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetShopWorkdayDetailList(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, 
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
    
    [HttpGet("shopWorkdayPeriodDetailList")]
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
    /// Handles the PATCH request to update a shop workday period detail.
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
        [FromBody, Required] ShopWorkdayPeriodDetail shopWorkdayPeriodDetail)
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
        shopWorkdayPeriodDetail.ModifiedBy = mondayUserData?.Data?.Me?.Name;
        
        // Implementation to update shop workday period detail
        var updatedShopWorkdayPeriodDetail = 
            await _posShopWorkdayPeriodDetailRepository.UpdateShopWorkdayPeriodDetailAsync(shopWorkdayPeriodDetail).ConfigureAwait(false);
        
        // If the shop workday period detail is not updated successfully, return a custom 400 Bad Request response with a custom error message.
        // otherwise, return an HTTP 200 OK response with the updated shop workday period detail.
        if (updatedShopWorkdayPeriodDetail == null)
        {
            return new CustomBadRequestResult("Failed to update shop workday period detail.");   
        }
        
        return Ok(updatedShopWorkdayPeriodDetail);
    }
    
    /// <summary>
    /// Handles the PATCH request to update a shop workday detail.
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
        [FromBody, Required] ShopWorkdayDetail shopWorkdayDetail)
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
        shopWorkdayDetail.ModifiedBy = mondayUserData?.Data?.Me?.Name;
        
        // Implementation to update shop workday detail
        var updatedShopWorkdayDetail = 
            await _posShopWorkdayDetailRepository.UpdateShopWorkdayDetailAsync(shopWorkdayDetail).ConfigureAwait(false);
        
        // If the shop workday detail is not updated successfully, return a custom 400 Bad Request response with a custom error message.
        // Otherwise, return an HTTP 200 OK response with the updated shop workday detail.
        if (updatedShopWorkdayDetail == null)
        {
            return new CustomBadRequestResult("Failed to update shop workday detail.");   
        }
        
        return Ok(updatedShopWorkdayDetail);
    }
    
    /// <summary>
    /// Handles the DELETE request to delete a shop workday detail.
    /// </summary>
    /// <param name="shopWorkdayDetail"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the shop workday detail is deleted successfully, it returns an HTTP 200 status code along with a boolean indicating success.
    /// - If there are transactions in the workday detail, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpDelete("deleteShopWorkdayDetail")]
    [ProducesResponseType(typeof(bool), 200)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> DeleteShopWorkdayDetail(
        [FromBody, Required] ShopWorkdayDetail shopWorkdayDetail)
    {
        // Validate the ID fields
        if (shopWorkdayDetail.AccountId == 0 || shopWorkdayDetail.ShopId == 0 || shopWorkdayDetail.WorkdayDetailId == 0)
        {
            return new CustomBadRequestResult("AccountId, ShopId and WorkdayDetailId must be greater than zero.");
        }
        
        // check if there are any transactions in the given workday detail id
        var txCount = 
            await _posTxSalesRepository.GetTxCountInWorkdayDetailIdAsync(shopWorkdayDetail.AccountId, shopWorkdayDetail.ShopId, shopWorkdayDetail.WorkdayDetailId).ConfigureAwait(false);
        
        // return error to user if there are transactions in the workday detail
        if (txCount > 0)
        {
            return new CustomBadRequestResult("Cannot delete workday detail with transactions.");
        }

        // Implementation to delete shop workday detail
        var isDeleted = 
            await _posShopWorkdayDetailRepository.DeleteShopWorkdayDetailAsync(shopWorkdayDetail).ConfigureAwait(false);
        
        return Ok(isDeleted);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the details of a specific transaction header.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction header is found, it returns an HTTP 200 status code along with the transaction header details.
    /// - If the transaction header is not found, it returns an HTTP 404 status code with a custom error message.
    /// </returns>
    [HttpGet("txSalesHeader")]
    [ProducesResponseType(typeof(TxSalesHeader), 200)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTxSalesHeader(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery, Required] int txSalesHeaderId)
    {
        // Implementation to fetch transaction header
        var txHeader = await _posTxSalesRepository.GetTxSalesHeaderAsync(accountId, shopId, txSalesHeaderId).ConfigureAwait(false);
        
        // If the transaction header is not found, return a custom 404 Not Found response with a custom error message.
        if (txHeader == null)
        {
            return new CustomNotFoundRequestResult("Transaction header not found.");
        }
        return Ok(txHeader);
    }
    
    
    /// <summary>
    /// Handles the GET request to fetch the list of transaction headers.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txDate"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction headers are found, it returns an HTTP 200 status code along with the transaction header details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("txSalesHeaderList")]
    [ProducesResponseType(typeof(IEnumerable<TxSalesHeader>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTxSalesHeaderList(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery, Required] DateTime txDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch transaction header list
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }
        
        // get the transaction header list from PosTxRepository
        var resultList = 
            await _posTxSalesRepository.GetTxSalesHeaderListAsync(accountId, shopId, txDate, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the list of transaction payments for a specific transaction header.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction payments are found, it returns an HTTP 200 status code along with the transaction payment details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("txPaymentList")]
    [ProducesResponseType(typeof(IEnumerable<TxPayment>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTxPaymentList(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery, Required] int txSalesHeaderId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch transaction payment list
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }
        
        // get the transaction payment list from PosTxRepository
        var resultList = 
            await _posTxSalesRepository.GetTxPaymentListAsync(accountId, shopId, txSalesHeaderId, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the details of a specific transaction payment.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txPaymentId"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction payment is found, it returns an HTTP 200 status code along with the transaction payment details.
    /// - If the transaction payment is not found, it returns an HTTP 404 status code with a custom error message.
    /// </returns>
    [HttpGet("txPayment")]
    [ProducesResponseType(typeof(TxPayment), 200)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTxPayment(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery,Required] int txPaymentId)
    {
        // Implementation to fetch transaction payment
        var txPayment = await _posTxSalesRepository.GetTxPaymentAsync(accountId, shopId, txPaymentId).ConfigureAwait(false);
        
        // If the transaction payment is not found, return a custom 404 Not Found response with a custom error message.
        if (txPayment == null)
        {
            return new CustomNotFoundRequestResult("Transaction payment not found.");
        }
        return Ok(txPayment);
    }
    
    /// <summary>
    /// Handles the PATCH request to update a transaction payment.
    /// </summary>
    /// <param name="txPayment"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction payment is updated successfully, it returns an HTTP 200 status code along with the updated transaction payment details.
    /// </returns>
    [HttpPatch("updateTxPayment")]
    [ProducesResponseType(typeof(TxPayment), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateTxPayment(
        [FromBody, Required] TxPayment txPayment)
    {
        // Validate the ID fields
        if (txPayment.AccountId == 0 || txPayment.ShopId == 0 || txPayment.TxPaymentId == 0)
        {
            return new CustomBadRequestResult("AccountId, ShopId and TxPaymentId must be greater than zero.");
        }

        // Get the Monday user data from the HttpContext
        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;

        if (mondayUserData == null)
        {
            // Handle the case where the data is not found
            return new CustomBadRequestResult("User data not found.");
        }
        
        // override the modified by field with the user name from Monday
        txPayment.ModifiedBy = mondayUserData?.Data?.Me?.Name;
        
        // Implementation to update transaction payment
        var updatedTxPayment = 
            await _posTxSalesRepository.UpdateTxPaymentAsync(txPayment).ConfigureAwait(false);
        
        // If the transaction payment is not updated successfully, return a custom 400 Bad Request response with a custom error message.
        // Otherwise, return an HTTP 200 OK response with the updated transaction payment details.
        if (updatedTxPayment == null)
        {
            return new CustomBadRequestResult("Failed to update transaction payment.");   
        }
        
        return Ok(updatedTxPayment);
    }
    
    /// <summary>
    /// Handles the GET request to fetch the list of payment methods.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the payment methods are found, it returns an HTTP 200 status code along with the payment method details.
    /// - If the page or pageSize is invalid, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("paymentMethodList")]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethod>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetPaymentMethodList(
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch payment method list
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }
        
        // get the payment method list from PosPaymentMethodRepository
        var resultList = 
            await _posPaymentMethodRepository.GetPaymentMethodListAsync(accountId, shopId, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
}