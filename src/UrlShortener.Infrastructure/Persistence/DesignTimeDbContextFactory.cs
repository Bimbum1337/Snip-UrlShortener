using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UrlShortener.Infrastructure.Persistence;

// Used by "dotnet ef" only.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=UrlShortenerDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=false";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = args.Length > 0 ? args[0] : DesignTimeConnectionString;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
