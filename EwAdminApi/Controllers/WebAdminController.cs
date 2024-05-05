using EwAdminApi.Extensions;
using EwAdminApi.Models.WebAdmin;
using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebAdminController : ControllerBase
{
    private readonly WebAdminCompanyMasterRepository _webAdminCompanyMasterRepository;

    public WebAdminController(WebAdminCompanyMasterRepository webAdminCompanyMasterRepository)
    {
        _webAdminCompanyMasterRepository = webAdminCompanyMasterRepository;
    }


    /// <summary>
    /// This method is responsible for fetching a list of companies.
    /// </summary>
    /// <param name="page">The page number to fetch. Defaults to 1 if not provided.</param>
    /// <param name="pageSize">The number of records per page. Defaults to 20 if not provided.</param>
    /// <returns>A list of companies for the given page and pageSize. If the page or pageSize is invalid, it returns a BadRequest. If the pageSize is more than 100, it also returns a BadRequest.</returns>
    [HttpGet("companylist")]
    [ProducesResponseType(typeof(IEnumerable<CompanyMaster>), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetCompanyList(
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

        // Fetch the list of companies from the repository.
        var resultList = await _webAdminCompanyMasterRepository.GetCompanyMasterListAsync(page, pageSize)
            .ConfigureAwait(false);

        // Return the fetched list of companies.
        return Ok(resultList);
    }
}