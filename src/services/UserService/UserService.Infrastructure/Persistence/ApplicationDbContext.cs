using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence.Configurations;

namespace UserService.Infrastructure.Persistence;

/// <summary>
/// Application database context for UserService.
/// Manages User and UserSession entities with EF Core.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<User> Users => Set<User>();
  public DbSet<UserSession> UserSessions => Set<UserSession>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply all entity configurations
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new UserSessionConfiguration());

    // Set default schema
    modelBuilder.HasDefaultSchema("public");
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    base.ConfigureConventions(configurationBuilder);

    // Configure string columns to use VARCHAR instead of TEXT by default
    configurationBuilder.Properties<string>()
        .HaveMaxLength(500);
  }
}
