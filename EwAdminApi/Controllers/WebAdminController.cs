using EwAdmin.Common.Models.WebAdmin;
using EwAdminApi.Extensions;
using EwAdminApi.Models.Monday;
using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebAdminController : ControllerBase
{
    private readonly WebAdminCompanyMasterRepository _webAdminCompanyMasterRepository;
    private readonly WebAdminRegionMasterRepository _webAdminRegionMasterRepository;
    private readonly WebAdminBrandMasterRepository _webAdminBrandMasterRepository;

    public WebAdminController(WebAdminCompanyMasterRepository webAdminCompanyMasterRepository,
        WebAdminRegionMasterRepository webAdminRegionMasterRepository,
        WebAdminBrandMasterRepository webAdminBrandMasterRepository)
    {
        _webAdminCompanyMasterRepository = webAdminCompanyMasterRepository;
        _webAdminRegionMasterRepository = webAdminRegionMasterRepository;
        _webAdminBrandMasterRepository = webAdminBrandMasterRepository;
    }


    /// <summary>
    /// Handles the GET request to fetch the list of companies.
    /// </summary>
    /// <param name="page">The page number to fetch. Defaults to 1 if not provided.</param>
    /// <param name="pageSize">The number of records per page. Defaults to 20 if not provided.</param>
    /// <param name="companyId">The company ID to filter the results. If not provided, all companies are fetched.</param>
    /// <returns>A list of companies for the given page and pageSize. If the page or pageSize is invalid, it returns a BadRequest. If the pageSize is more than 100, it also returns a BadRequest.</returns>
    [HttpGet("companylist")]
    [ProducesResponseType(typeof(IEnumerable<CompanyMaster>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetCompanyList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? companyId = null)
    {
        // Check if the page or pageSize is less than or equal to 0. If so, return a BadRequest.
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        // Check if the pageSize is more than 100. If so, return a BadRequest.
        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }

        // Fetch the list of companies from the repository.
        var resultList = await _webAdminCompanyMasterRepository
            .GetCompanyMasterListAsync(page, pageSize, companyId)
            .ConfigureAwait(false);

        // Return the fetched list of companies.
        return Ok(resultList);
    }

    // add an API endpoint (hello) to return the HttpContext.Items["MondayUserData"] if it exists
    // code here
    /// <summary>
    /// Handles the GET request to fetch current user data from HttpContext.Items.
    /// </summary>
    /// <returns>
    /// Returns the MondayUserData from HttpContext.Items if it exists, otherwise returns a BadRequest.
    /// </returns>
    [HttpGet("hello")]
    [ProducesResponseType(typeof(MondayUserData), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public IActionResult GetMondayUserData()
    {
        if (HttpContext.Items.ContainsKey("MondayUserData"))
        {
            return Ok(HttpContext.Items["MondayUserData"]);
        }

        return new CustomBadRequestResult("MondayUserData not found in HttpContext.Items.");
    }

    /// <summary>
    /// Handles the GET request to fetch the list of regions.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// An IActionResult containing the list of regions for the given page and pageSize.
    /// - If the region list is fetched successfully, it returns an Ok result with the list of regions.
    /// - If the page or pageSize is invalid, it returns a BadRequest.
    /// </returns>
    [HttpGet("regionlist")]
    [ProducesResponseType(typeof(IEnumerable<RegionMasterDatabaseMetadata>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetRegionList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Check if the page or pageSize is less than or equal to 0. If so, return a BadRequest.
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        // Check if the pageSize is more than 100. If so, return a BadRequest.
        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }

        // Fetch the list of regions from the repository.
        var resultList = await _webAdminRegionMasterRepository
            .GetRegionListAsync(page, pageSize)
            .ConfigureAwait(false);

        // Return the fetched list of regions.
        return Ok(resultList);
    }
    
    // add an API endpoint (brandlist) to return the list of brands
    // code here
    
    [HttpGet("brandlist")]
    [ProducesResponseType(typeof(IEnumerable<BrandMaster>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetBrandList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? companyId = null, [FromQuery] int? brandId = null)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return new CustomBadRequestResult("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return new CustomBadRequestResult("PageSize must be smaller or equal to 100.");
        }

        var resultList = await _webAdminBrandMasterRepository
            .GetBrandMasterListAsync(page, pageSize, companyId, brandId)
            .ConfigureAwait(false);

        return Ok(resultList);
    }
}