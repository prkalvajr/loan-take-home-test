using Loans.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Loans.Api.ErrorHandling;

/// <summary>
/// Turns client mistakes that surface as exceptions into 400 ProblemDetails:
/// <list type="bullet">
/// <item><see cref="BadHttpRequestException"/>: an empty or malformed JSON body.</item>
/// <item><see cref="DomainException"/>: invalid data that got past request validation.</item>
/// </list>
/// Any other exception is left to the default handler, which returns 500 without internal details.
/// </summary>
public sealed class BadRequestExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var detail = exception switch
        {
            BadHttpRequestException => "The request body is missing or is not valid JSON.",
            DomainException domain => domain.Message,
            _ => null,
        };

        if (detail is null)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = StatusCodes.Status400BadRequest, Title = "Invalid application", Detail = detail },
        });
    }
}
