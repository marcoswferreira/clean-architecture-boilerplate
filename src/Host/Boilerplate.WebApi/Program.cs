using Boilerplate.WebApi.Extensions;
using Crosscutting.SqlServer.Extensions;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure ────────────────────────────────────────────────────────────
// Registers ApplicationDbContext with the SQL Server provider.
// Reads the "Default" connection string from appsettings.json.
builder.Services.AddSqlServerInfrastructure(builder.Configuration);

// ── Presentation / API ───────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioningExtensions();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

var app = builder.Build();

// ── Database Migration ────────────────────────────────────────────────────────
// Automatically applies any pending EF Core migrations before the app starts
// serving requests. Safe to call on every startup — it's a no-op when the DB
// is already up-to-date.
await app.MigrateDatabaseAsync();

// ── HTTP Pipeline ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

