using Loans.Api.Endpoints;
using Loans.Api.ErrorHandling;
using Loans.Application;
using Loans.Infrastructure;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Loans")
    ?? throw new InvalidOperationException("Connection string 'Loans' is missing.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().WithMethods("POST")));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();

// Validates request bodies from their DataAnnotations and returns 400 with errors per field.
builder.Services.AddValidation();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler();
app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapApplicationEndpoints();

await app.RunAsync();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
