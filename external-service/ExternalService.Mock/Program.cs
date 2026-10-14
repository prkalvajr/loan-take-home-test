using ExternalService.Mock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ApplicationStore>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapApplicationEndpoints();

app.Run();
