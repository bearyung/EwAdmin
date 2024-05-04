using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PosAdminController : ControllerBase
{
    private readonly PosShopRepository _posShopRepository;
    private readonly PosShopWorkdayDetailRepository _posShopWorkdayDetailRepository;

    public PosAdminController(
        PosShopRepository posShopRepository,
        PosShopWorkdayDetailRepository posShopWorkdayDetailRepository)
    {
        _posShopRepository = posShopRepository;
        _posShopWorkdayDetailRepository = posShopWorkdayDetailRepository;
    }

    [HttpGet("getshopdetails")]
    public async Task<IActionResult> GetShopDetail(
        [FromQuery] int accountId, [FromQuery] int shopId)
    {
        // Implementation to fetch shop details
        var shop = await _posShopRepository.GetShopDetailAsync(accountId, shopId);
        if (shop == null)
        {
            return NotFound(new { message = "Shop not found." });
        }
        return Ok(shop);
    }

    [HttpGet("getshopwokdaydetail")]
    public async Task<IActionResult> GetShopWorkdayDetailList(
        [FromQuery] int accountId, [FromQuery] int shopId, 
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch shop workday detail
        // if user has not input the start date, assume to fetch the last 5 records 
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return BadRequest("PageSize must be smaller or equal to 100.");
        }
        
        // get the shop workday detail list from PosShopWorkdayDetailRepository
        var resultList = 
            await _posShopWorkdayDetailRepository.GetShopWorkdayDetailListAsync(accountId, shopId, startDate, endDate, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
}