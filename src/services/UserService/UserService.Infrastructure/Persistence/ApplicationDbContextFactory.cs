using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating ApplicationDbContext instances.
/// This is used by EF Core tools for migrations without needing to run the full application.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

    // Use a default connection string for design-time operations
    // This won't be used in production - only for migrations
    optionsBuilder.UseNpgsql(
        "Host=localhost;Port=5432;Database=userservice;Username=postgres;Password=postgres",
        npgsqlOptions =>
        {
          npgsqlOptions.MigrationsAssembly("UserService.Infrastructure");
          npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });

    return new ApplicationDbContext(optionsBuilder.Options);
  }
}
