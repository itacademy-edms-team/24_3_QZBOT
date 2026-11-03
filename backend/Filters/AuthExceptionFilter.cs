using backend.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Filters;

public sealed class AuthExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case IdentityValidationException validation:
                var duplicate = validation.Errors.Any(error =>
                    error.Code is "DuplicateUserName" or "DuplicateEmail");
                context.Result = new ObjectResult(new
                {
                    errors = validation.Errors.Select(error => new { error.Code, error.Description })
                })
                {
                    StatusCode = duplicate
                        ? StatusCodes.Status409Conflict
                        : StatusCodes.Status400BadRequest
                };
                break;

            case InvalidCredentialsException:
                context.Result = new ObjectResult(new { error = "Invalid email or password." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                break;

            default:
                return;
        }

        context.ExceptionHandled = true;
    }
}
