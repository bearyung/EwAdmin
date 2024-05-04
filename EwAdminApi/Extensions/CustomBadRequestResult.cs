using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Extensions;

public class CustomBadRequestResult : BadRequestObjectResult
{
    public CustomBadRequestResult(string error)
        : base(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = StatusCodes.Status400BadRequest,
            errors = new { message = error },
            traceId = Activity.Current?.Id ?? string.Empty
        })
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }
}