
using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Extensions;

public class CustomNotFoundRequestResult : NotFoundObjectResult
{
    public CustomNotFoundRequestResult(string error)
        : base(new CustomErrorRequestResultDto
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            Title = "The requested resource was not found.",
            Status = StatusCodes.Status404NotFound,
            Errors = new { message = error }
        })
    {
        StatusCode = StatusCodes.Status404NotFound;
    }
}