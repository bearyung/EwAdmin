using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WebAdminController : ControllerBase
{
    private readonly WebAdminCompanyMasterRepository _webAdminCompanyMasterRepository;

    public WebAdminController(WebAdminCompanyMasterRepository webAdminCompanyMasterRepository)
    {
        _webAdminCompanyMasterRepository = webAdminCompanyMasterRepository;
    }

    [HttpGet("GetAllCompanies")]
    public async Task<IActionResult> GetAllCompanies(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("Page and PageSize must be greater than zero.");
        }

        if (pageSize > 100)
        {
            return BadRequest("PageSize must be smaller or equal to 100.");
        }

        var resultList = await _webAdminCompanyMasterRepository.GetCompanyMasterListAsync(page, pageSize).ConfigureAwait(false);
        return Ok(resultList);
    }
}