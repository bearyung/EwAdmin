using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Extensions;

public class CustomBadRequestResult : BadRequestObjectResult
{
    public CustomBadRequestResult(string error)
        : base(new CustomErrorRequestResultDto
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Errors = new { message = error },
            TraceId = Activity.Current?.Id ?? string.Empty
        })
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }
}