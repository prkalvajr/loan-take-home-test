namespace ExternalService.Mock;

/// <summary>
/// Returns 500 for a share of write requests so the caller's retry logic can be shown in the demo.
/// FAIL_RATE=0 never fails (default), FAIL_RATE=1 always fails, FAIL_RATE=0.5 fails about half the time.
/// </summary>
public sealed class FailureInjectionFilter(IConfiguration configuration, ILogger<FailureInjectionFilter> logger)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var failRate = configuration.GetValue<double>("FAIL_RATE");
        if (failRate > 0 && Random.Shared.NextDouble() < failRate)
        {
            logger.LogWarning("Simulated failure for {Method} {Path} (FAIL_RATE={FailRate})",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path, failRate);
            return Results.Problem("Simulated failure", statusCode: StatusCodes.Status500InternalServerError);
        }

        return await next(context);
    }
}
