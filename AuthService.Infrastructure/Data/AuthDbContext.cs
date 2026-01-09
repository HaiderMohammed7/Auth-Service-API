using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(100);

                entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UX_Users_Email");
                entity.HasIndex(u => u.UserName).IsUnique().HasDatabaseName("UX_Users_UserName");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role).WithMany(u => u.UserRoles)
               .HasForeignKey(ur => ur.RoleID).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ur => new { ur.UserID, ur.RoleID }).IsUnique();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);

                entity.HasIndex(rt => rt.Token).IsUnique();
            });

            modelBuilder.Entity<LoginAttempt>(entity =>
            {
                entity.Property(la => la.Email).IsRequired().HasMaxLength(256);

                entity.HasIndex(la => la.Email);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.UserID);
            });
        }
    }
}