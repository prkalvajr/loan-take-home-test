using Loans.Api.Endpoints;
using Loans.Api.ErrorHandling;
using Loans.Application;
using Loans.Infrastructure;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Loans")
    ?? throw new InvalidOperationException("Connection string 'Loans' is missing.");

// Trailing slash so relative paths like "applications" resolve under the base URL.
var externalServiceBaseUrl = new Uri(
    (builder.Configuration["ExternalService:BaseUrl"] ?? throw new InvalidOperationException("ExternalService:BaseUrl is missing."))
        .TrimEnd('/') + "/");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString, externalServiceBaseUrl);

if (builder.Configuration.GetValue("Outbox:WorkerEnabled", defaultValue: true))
    builder.Services.AddOutboxWorker();

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
