using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BidNet.Controllers;

[ApiController]
[Authorize]
public class ApiController : ControllerBase
{
    protected IActionResult HandleErrors(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Problem();
        }

        HttpContext.Items["errors"] = errors;
        if (errors.Count > 1)
        {
            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var modelStateDictionary = new ModelStateDictionary();
                foreach (var error in errors)
                {
                    modelStateDictionary.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem(modelStateDictionary);
            }
        }

        return Problem(errors[0]);
    }

    private IActionResult Problem(Error error)
    {
        var statusCode = MapToStatusCode(error);
        return Problem(statusCode: statusCode, title: error.Description);
    }

    private static int MapToStatusCode(Error error)
    {
        var errorType = error.Type;

        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

}