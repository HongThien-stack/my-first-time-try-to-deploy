using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserLoginLog> UserLoginLogs { get; set; } = null!;
    public DbSet<UserAuditLog> UserAuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsSystem).HasColumnName("is_system");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Name).HasDatabaseName("IX_roles_name");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500);
            entity.Property(e => e.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at");
            entity.Property(e => e.OtpCode).HasColumnName("otp_code").HasMaxLength(10);
            entity.Property(e => e.OtpPurpose).HasColumnName("otp_purpose").HasMaxLength(50);
            entity.Property(e => e.OtpExpiresAt).HasColumnName("otp_expires_at");
            entity.Property(e => e.OtpAttempts).HasColumnName("otp_attempts");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");

            entity.HasIndex(e => e.Email).HasDatabaseName("IX_users_email").IsUnique();
            entity.HasIndex(e => e.Phone).HasDatabaseName("IX_users_phone");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("IX_users_role_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_users_status");
            entity.HasIndex(e => e.RefreshToken).HasDatabaseName("IX_users_refresh_token");
            entity.HasIndex(e => e.OtpCode).HasDatabaseName("IX_users_otp_code");

            // Configure relationship with Role
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .HasConstraintName("FK_users_roles")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserLoginLog entity
        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.ToTable("user_login_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.LoginAt).HasColumnName("login_at").IsRequired();
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason").HasMaxLength(255);

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_login_logs_user_id");
            entity.HasIndex(e => e.LoginAt).HasDatabaseName("IX_login_logs_login_at");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_login_logs_status");

            // Configure relationship with User
            entity.HasOne(e => e.User)
                .WithMany(u => u.LoginLogs)
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_login_logs_users")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserAuditLog entity
        modelBuilder.Entity<UserAuditLog>(entity =>
        {
            entity.ToTable("user_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by").IsRequired();
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.OldValues).HasColumnName("old_values");
            entity.Property(e => e.NewValues).HasColumnName("new_values");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.PerformedAt).HasColumnName("performed_at").IsRequired();

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_audit_logs_user_id");
            entity.HasIndex(e => e.PerformedBy).HasDatabaseName("IX_audit_logs_performed_by");
            entity.HasIndex(e => e.Action).HasDatabaseName("IX_audit_logs_action");
            entity.HasIndex(e => e.EntityType).HasDatabaseName("IX_audit_logs_entity_type");
            entity.HasIndex(e => e.PerformedAt).HasDatabaseName("IX_audit_logs_performed_at");

            // Configure relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogsAsUser)
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_audit_logs_user")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Performer)
                .WithMany(u => u.AuditLogsAsPerformer)
                .HasForeignKey(e => e.PerformedBy)
                .HasConstraintName("FK_audit_logs_performed_by")
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
