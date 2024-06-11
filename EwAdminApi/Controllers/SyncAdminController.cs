using EwAdminApi.Extensions;
using EwAdminApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncAdminController : ControllerBase
{
    private readonly PosSyncRepository _posSyncRepository;

    public SyncAdminController(PosSyncRepository posSyncRepository)
    {
        _posSyncRepository = posSyncRepository;
    }

    /// <summary>
    /// Handles the GET request to start data sync for a specific region.
    /// </summary>
    /// <param name="regionId"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the data sync is started successfully, it returns an HTTP 200 status code along with a success message.
    /// - If the data sync fails to start, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("stopDataSync")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> StopDataSync([FromQuery] int regionId)
    {
        var result = await _posSyncRepository.StopDataSync(regionId).ConfigureAwait(false);
        return result
            ? Ok($"Data sync for region {regionId} stopped successfully")
            : new CustomBadRequestResult($"Failed to stop data sync for region {regionId}");
    }
    
    /// <summary>
    /// Handles the GET request to get the status of data sync for a specific region.
    /// </summary>
    /// <param name="regionId"></param>
    /// <returns>
    /// An IActionResult that represents the result of the action method:
    /// - If the data sync status is fetched successfully, it returns an HTTP 200 status code along with the status message.
    /// - If the data sync status fails to fetch, it returns an HTTP 400 status code with a custom error message.
    /// </returns>
    [HttpGet("resumeDataSync")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> ResumeDataSync([FromQuery] int regionId)
    {
        var result = await _posSyncRepository.ResumeDataSync(regionId).ConfigureAwait(false);
        return result
            ? Ok($"Data sync for region {regionId} resumed successfully")
            : new CustomBadRequestResult($"Failed to resume data sync for region {regionId}");
    }
    
    // Implement the GetDataSyncStatus method
    // code here
    [HttpGet("getDataSyncStatus")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(CustomErrorRequestResultDto), 400)]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetDataSyncStatus([FromQuery] int regionId)
    {
        var result = await _posSyncRepository.GetDataSyncStatus(regionId).ConfigureAwait(false);
        return result != "Error"
            ? Ok(new { regionId, syncReady = result})
            : new CustomBadRequestResult($"Failed to get data sync status for region {regionId}");
    }
}