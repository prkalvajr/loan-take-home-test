using System.Diagnostics;
using Loans.Api.Contracts;
using Loans.Application.UseCases.SubmitApplication;

namespace Loans.Api.Endpoints;

/// <summary>Thin HTTP layer: request to command, result to response. No rules and no database access.</summary>
public static class ApplicationEndpoints
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications", SubmitAsync)
            .WithName("SubmitApplication")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // Request validation runs before this method (AddValidation); an invalid body never gets here.
    private static async Task<IResult> SubmitAsync(
        SubmitApplicationRequest request, SubmitApplicationUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(request.ToCommand(), cancellationToken);

        return result switch
        {
            SubmitApplicationResult.Approved approved => Results.Ok(ApprovedApplicationResponse.From(approved)),
            SubmitApplicationResult.Denied denied => Results.Ok(DeniedApplicationResponse.From(denied)),
            SubmitApplicationResult.Conflict => Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Application already being submitted",
                detail: "Another submission with this SSN was saved at the same moment. Please submit again."),
            _ => throw new UnreachableException($"Unhandled result {result.GetType().Name}"),
        };
    }
}
