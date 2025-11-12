using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for User entity.
/// Configures table schema, indexes, and value object conversions.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    // Table configuration
    builder.ToTable("users");

    // Primary key
    builder.HasKey(u => u.Id);

    builder.Property(u => u.Id)
        .HasColumnName("id")
        .ValueGeneratedNever(); // Guid generated in domain

    // Email value object
    builder.Property(u => u.Email)
        .HasColumnName("email")
        .HasMaxLength(255)
        .IsRequired()
        .HasConversion(
            email => email.Value,
            value => Email.Create(value));

    builder.HasIndex(u => u.Email)
        .IsUnique()
        .HasDatabaseName("ix_users_email");

    // Password value object (stores hash only)
    builder.Property(u => u.Password)
        .HasColumnName("password_hash")
        .HasMaxLength(255)
        .IsRequired()
        .HasConversion(
            password => password.HashedValue,
            hash => Password.FromHash(hash));

    // Simple properties
    builder.Property(u => u.FullName)
        .HasColumnName("full_name")
        .HasMaxLength(200)
        .IsRequired();

    builder.Property(u => u.IsEmailVerified)
        .HasColumnName("is_email_verified")
        .IsRequired()
        .HasDefaultValue(false);

    builder.Property(u => u.IsActive)
        .HasColumnName("is_active")
        .IsRequired()
        .HasDefaultValue(true);

    builder.Property(u => u.FailedLoginAttempts)
        .HasColumnName("failed_login_attempts")
        .IsRequired()
        .HasDefaultValue(0);

    builder.Property(u => u.LockedOutUntil)
        .HasColumnName("locked_out_until")
        .IsRequired(false);

    builder.Property(u => u.LastLoginAt)
        .HasColumnName("last_login_at")
        .IsRequired(false);

    builder.Property(u => u.CreatedAt)
        .HasColumnName("created_at")
        .IsRequired();

    builder.Property(u => u.UpdatedAt)
        .HasColumnName("updated_at")
        .IsRequired();

    // Index for lockout queries
    builder.HasIndex(u => u.LockedOutUntil)
        .HasDatabaseName("ix_users_locked_out_until")
        .HasFilter("locked_out_until IS NOT NULL");

    // Index for active users
    builder.HasIndex(u => u.IsActive)
        .HasDatabaseName("ix_users_is_active");

    // Relationships
    builder.HasMany(u => u.Sessions)
        .WithOne()
        .HasForeignKey(s => s.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    // Ignore domain events (not persisted)
    builder.Ignore(u => u.DomainEvents);
  }
}
