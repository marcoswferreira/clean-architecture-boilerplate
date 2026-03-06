using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Crosscutting.SqlServer.DbContexts;

/// <summary>
/// Design-time factory for <see cref="ApplicationDbContext"/>.
///
/// WHY THIS IS NEEDED:
///   The EF Core CLI tools (e.g., "dotnet ef migrations add") need to instantiate
///   the DbContext at design time — without running the full application host,
///   so the DI container is not available.
///
///   Implementing IDesignTimeDbContextFactory tells the tools: "use this factory
///   to create the context instead of trying to resolve it from DI."
///
/// CONNECTION STRING:
///   Reads from the WebApi's appsettings.json (or appsettings.Development.json)
///   using the same key the runtime uses: ConnectionStrings:Default.
///   This means you only ever configure the connection string in ONE place.
///
/// RUNTIME:
///   At runtime this class is never called. The context is always built through
///   DI via AddSqlServerInfrastructure() in Program.cs.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Walk up from Crosscutting.SqlServer to the solution root,
        // then down into the WebApi host project where appsettings.json lives.
        // This mirrors the path that "dotnet ef" resolves at design time.
        // EF Core CLI sets the working directory to the startup project's root
        // (src/Host/Boilerplate.WebApi/) when running design-time commands.
        // Directory.GetCurrentDirectory() is therefore the reliable way to locate
        // appsettings.json without fragile path traversal from AppContext.BaseDirectory.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found in appsettings.json. " +
                "Make sure src/Host/Boilerplate.WebApi/appsettings.json contains " +
                "a ConnectionStrings:Default entry.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
