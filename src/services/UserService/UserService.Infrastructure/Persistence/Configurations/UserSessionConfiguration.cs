using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for UserSession entity.
/// Configures table schema, indexes, and relationships.
/// </summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
  public void Configure(EntityTypeBuilder<UserSession> builder)
  {
    // Table configuration
    builder.ToTable("user_sessions");

    // Primary key
    builder.HasKey(s => s.Id);

    builder.Property(s => s.Id)
        .HasColumnName("id")
        .ValueGeneratedNever(); // Guid generated in domain

    // Foreign key (relationship defined in UserConfiguration)
    builder.Property(s => s.UserId)
        .HasColumnName("user_id")
        .IsRequired();

    // Session properties
    builder.Property(s => s.RefreshToken)
        .HasColumnName("refresh_token")
        .HasMaxLength(255)
        .IsRequired();

    builder.HasIndex(s => s.RefreshToken)
        .IsUnique()
        .HasDatabaseName("ix_user_sessions_refresh_token");

    builder.Property(s => s.IpAddress)
        .HasColumnName("ip_address")
        .HasMaxLength(45) // IPv6 max length
        .IsRequired(false);

    builder.Property(s => s.UserAgent)
        .HasColumnName("user_agent")
        .HasMaxLength(500)
        .IsRequired(false);

    builder.Property(s => s.IsActive)
        .HasColumnName("is_active")
        .IsRequired()
        .HasDefaultValue(true);

    builder.Property(s => s.RevokedAt)
        .HasColumnName("revoked_at")
        .IsRequired(false);

    builder.Property(s => s.CreatedAt)
        .HasColumnName("created_at")
        .IsRequired();

    builder.Property(s => s.ExpiresAt)
        .HasColumnName("expires_at")
        .IsRequired();

    builder.Property(s => s.LastUsedAt)
        .HasColumnName("last_used_at")
        .IsRequired(false);

    builder.Property(s => s.UpdatedAt)
        .HasColumnName("updated_at")
        .IsRequired(false);

    // Indexes for common queries
    builder.HasIndex(s => s.UserId)
        .HasDatabaseName("ix_user_sessions_user_id");

    builder.HasIndex(s => s.ExpiresAt)
        .HasDatabaseName("ix_user_sessions_expires_at");

    builder.HasIndex(s => new { s.UserId, s.IsActive })
        .HasDatabaseName("ix_user_sessions_user_id_is_active");

    // Composite index for active sessions
    builder.HasIndex(s => new { s.UserId, s.IsActive, s.ExpiresAt })
        .HasDatabaseName("ix_user_sessions_active");
  }
}
