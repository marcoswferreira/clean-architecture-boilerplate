using Crosscutting.SqlServer.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crosscutting.SqlServer.Extensions;

/// <summary>
/// Extension methods for registering and migrating the SQL Server database.
///
/// USAGE (in Program.cs):
///   builder.Services.AddSqlServerInfrastructure(builder.Configuration);
///   ...
///   await app.MigrateDatabaseAsync();
/// </summary>
public static class SqlServerExtensions
{
    /// <summary>
    /// Registers <see cref="ApplicationDbContext"/> with the DI container using the
    /// "Default" connection string from configuration.
    ///
    /// Call this in the service registration section of Program.cs BEFORE app.Build().
    /// </summary>
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found in configuration. " +
                "Add it to appsettings.json under ConnectionStrings:Default.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    /// <summary>
    /// Automatically applies any pending EF Core migrations at application startup.
    ///
    /// HOW IT WORKS:
    ///   EF Core tracks which migrations have already been applied in a special table
    ///   named "__EFMigrationsHistory". On startup this method:
    ///     1. Queries that table to compare applied vs. available migrations.
    ///     2. Runs any migrations that are pending (DDL statements: CREATE TABLE, ALTER TABLE, …).
    ///     3. Returns immediately if the database is already up-to-date.
    ///
    /// WHY USE THIS INSTEAD OF "dotnet ef database update":
    ///   Automatic migration is ideal for templates, demos, and CD pipelines where you
    ///   want zero manual steps. For production environments with strict change control,
    ///   you may prefer to run migrations as a separate deployment step.
    ///
    /// Call this AFTER app.Build() and BEFORE app.Run().
    /// </summary>
    public static async Task MigrateDatabaseAsync(this IHost app)
    {
        // Create a scope so we can resolve scoped services (DbContext is scoped by default).
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // GetPendingMigrationsAsync() returns the names of migrations that have NOT
            // yet been applied to the database.
            var pending = await db.Database.GetPendingMigrationsAsync();
            var pendingList = pending.ToList();

            if (pendingList.Count == 0)
            {
                logger.LogInformation("Database is up-to-date. No pending migrations.");
                return;
            }

            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
                pendingList.Count, string.Join(", ", pendingList));

            // MigrateAsync() applies all pending migrations inside a transaction.
            // If any migration fails, the transaction is rolled back.
            await db.Database.MigrateAsync();

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw; // Re-throw so the app fails fast rather than running with a broken schema.
        }
    }
}
