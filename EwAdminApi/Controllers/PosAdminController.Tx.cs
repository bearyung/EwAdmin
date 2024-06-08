using System.ComponentModel.DataAnnotations;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

public partial class PosAdminController
{
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
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId,
        [FromQuery, Required] int txSalesHeaderId)
    {
        // Implementation to fetch transaction header
        var txHeader = await _posTxSalesRepository.GetTxSalesHeaderAsync(accountId, shopId, txSalesHeaderId)
            .ConfigureAwait(false);

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
    /// <param name="txDateGte"></param>
    /// <param name="txDateLte"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="tableCode"></param>
    /// <param name="cusCountGte"></param>
    /// <param name="amountTotalGte"></param>
    /// <param name="amountTotalLte"></param>
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
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId,
        [FromQuery] DateTime? txDateGte, [FromQuery] DateTime? txDateLte,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] int? txSalesHeaderId = null, [FromQuery] string? tableCode = null,
        [FromQuery] int? cusCountGte = null, [FromQuery] decimal? amountTotalGte = null,
        [FromQuery] decimal? amountTotalLte = null)
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
        var resultList = await _posTxSalesRepository
            .GetTxSalesHeaderListAsync(accountId, shopId, txDateGte, txDateLte, page, pageSize, txSalesHeaderId,
                tableCode,
                cusCountGte, amountTotalGte, amountTotalLte)
            .ConfigureAwait(false);

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
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId,
        [FromQuery, Required] int txSalesHeaderId,
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
            await _posTxSalesRepository.GetTxPaymentListAsync(accountId, shopId, txSalesHeaderId, page, pageSize)
                .ConfigureAwait(false);

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
        [FromQuery, Required] int accountId, [FromQuery, Required] int shopId, [FromQuery, Required] int txPaymentId)
    {
        // Implementation to fetch transaction payment
        var txPayment = await _posTxSalesRepository.GetTxPaymentAsync(accountId, shopId, txPaymentId)
            .ConfigureAwait(false);

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

        // override the modified by field with the username from Monday
        txPayment.ModifiedBy = mondayUserData.Data?.Me?.Name;
        txPayment.ModifiedDate = DateTime.Now;

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
    /// Handles the PATCH request to update a transaction header.
    /// </summary>
    /// <param name="txSalesHeader">The transaction header to be updated. Only tableId, tableCode, sectionId, sectionName, cusCount, and enabled fields are updated.</param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the transaction header is updated successfully, it returns an HTTP 200 status code along with the updated transaction header.
    /// - If the user data is not found, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpPatch("updateTxSalesHeader")]
    [ProducesResponseType(typeof(TxSalesHeader), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateTxSalesHeader(
        [JsonBinder, Required] TxSalesHeader txSalesHeader)
    {
        // Validate the ID fields
        if (txSalesHeader.AccountId == 0 || txSalesHeader.ShopId == 0 || txSalesHeader.TxSalesHeaderId == 0)
        {
            return new CustomBadRequestResult("AccountId, ShopId and TxSalesHeaderId must be greater than zero.");
        }

        // Get the Monday user data from the HttpContext
        var mondayUserData = HttpContext.Items["MondayUserData"] as MondayUserResponse;

        if (mondayUserData == null)
        {
            // Handle the case where the data is not found
            return new CustomBadRequestResult("User data not found.");
        }

        // override the modified by field with the username from Monday
        txSalesHeader.ModifiedBy = mondayUserData.Data?.Me?.Name;
        txSalesHeader.ModifiedDate = DateTime.Now;

        // Implementation to update transaction header
        var updatedTxSalesHeader =
            await _posTxSalesRepository.UpdateTxSalesHeaderAsync(txSalesHeader).ConfigureAwait(false);

        // If the transaction header is not updated successfully, return a custom 400 Bad Request response with a custom error message.
        // Otherwise, return an HTTP 200 OK response with the updated transaction header.
        if (updatedTxSalesHeader == null)
        {
            return new CustomBadRequestResult("Failed to update transaction header.");
        }

        return Ok(updatedTxSalesHeader);
    }
}