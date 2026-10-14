const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().WithMethods("POST")));

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(FrontendCorsPolicy);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
