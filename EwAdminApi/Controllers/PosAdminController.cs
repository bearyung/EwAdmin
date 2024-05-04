using System.ComponentModel.DataAnnotations;
using EwAdminApi.Extensions;
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
    
    [HttpGet("shop")]
    public async Task<IActionResult> GetShopDetail(
        [FromQuery] int accountId, [FromQuery] int shopId)
    {
        // Implementation to fetch shop details
        var shop = await _posShopRepository.GetShopDetailAsync(accountId, shopId);
        if (shop == null)
        {
            return new CustomNotFoundRequestResult("Shop not found.");
        }
        return Ok(shop);
    }

    [HttpGet("shopworkdaydetaillist")]
    public async Task<IActionResult> GetShopWorkdayDetailList(
        [FromQuery] int accountId, [FromQuery] int shopId, 
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Implementation to fetch shop workday detail
        // if user has not input the start date, assume to fetch the last 5 records 
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }
        
        // get the shop workday detail list from PosShopWorkdayDetailRepository
        var resultList = 
            await _posShopWorkdayDetailRepository.GetShopWorkdayDetailListAsync(accountId, shopId, startDate, endDate, page, pageSize).ConfigureAwait(false);
        
        return Ok(resultList);
    }
    
    [HttpGet("shopworkdayperioddetaillist")]
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
}