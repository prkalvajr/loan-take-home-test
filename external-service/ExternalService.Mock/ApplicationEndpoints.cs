namespace ExternalService.Mock;

public static class ApplicationEndpoints
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/applications");

        group.MapGet("/", (ApplicationStore store) => Results.Ok(store.All()));

        group.MapPost("/", (ApplicationPayload payload, HttpRequest request, ApplicationStore store,
            TimeProvider clock, ILogger<ApplicationStore> logger) =>
        {
            var key = request.Headers[IdempotencyKeyHeader].ToString();
            if (IsRepeated(key, store, logger))
                return Results.Ok();

            if (!store.TryCreate(payload, clock.GetUtcNow()))
            {
                // Same application ID sent again: already stored, so this is a success, not a duplicate record.
                logger.LogInformation("Application {ApplicationId} already exists, nothing created", payload.ApplicationId);
                store.RememberDelivery(key);
                return Results.Ok();
            }

            logger.LogInformation("Application {ApplicationId} created (key {IdempotencyKey})", payload.ApplicationId, key);
            store.RememberDelivery(key);
            return Results.Ok();
        }).AddEndpointFilter<FailureInjectionFilter>();

        group.MapPut("/{applicationId:guid}", (Guid applicationId, ApplicationPayload payload, HttpRequest request,
            ApplicationStore store, TimeProvider clock, ILogger<ApplicationStore> logger) =>
        {
            if (applicationId != payload.ApplicationId)
                return Results.BadRequest(new { error = "applicationId in the route and body must match" });

            var key = request.Headers[IdempotencyKeyHeader].ToString();
            if (IsRepeated(key, store, logger))
                return Results.Ok();

            if (!store.TryReplace(payload, clock.GetUtcNow()))
            {
                logger.LogWarning("Application {ApplicationId} not found for update", applicationId);
                return Results.NotFound();
            }

            logger.LogInformation("Application {ApplicationId} updated (key {IdempotencyKey})", applicationId, key);
            store.RememberDelivery(key);
            return Results.Ok();
        }).AddEndpointFilter<FailureInjectionFilter>();
    }

    private static bool IsRepeated(string key, ApplicationStore store, ILogger logger)
    {
        if (string.IsNullOrEmpty(key) || !store.IsRepeatedDelivery(key))
            return false;

        logger.LogInformation("Repeated delivery for key {IdempotencyKey} ignored", key);
        return true;
    }
}
