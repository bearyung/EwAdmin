using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Extensions;

public class CustomNotFoundRequestResult : NotFoundObjectResult
{
    public CustomNotFoundRequestResult(string error)
        : base(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            title = "The requested resource was not found.",
            status = StatusCodes.Status404NotFound,
            errors = new { message = error }
        })
    {
        StatusCode = StatusCodes.Status404NotFound;
    }
}