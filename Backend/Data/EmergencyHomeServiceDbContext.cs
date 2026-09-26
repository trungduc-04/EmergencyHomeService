using System;
using System.Collections.Generic;
using EmergencyHomeService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmergencyHomeService.API.Data;

public partial class EmergencyHomeServiceDbContext : DbContext
{
    public EmergencyHomeServiceDbContext(DbContextOptions<EmergencyHomeServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RequestImage> RequestImages { get; set; }

    public virtual DbSet<RequestStatusHistory> RequestStatusHistories { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkerLocation> WorkerLocations { get; set; }

    public virtual DbSet<WorkerProfile> WorkerProfiles { get; set; }

    public virtual DbSet<WorkerService> WorkerServices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.Property(e => e.CustomerId).ValueGeneratedNever();

            entity.HasOne(d => d.Customer).WithOne(p => p.CustomerProfile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerProfiles_Users");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Notifications_CreatedAt");

            entity.HasOne(d => d.Request).WithMany(p => p.Notifications).HasConstraintName("FK_Notifications_Request");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Ratings_CreatedAt");

            entity.HasOne(d => d.Customer).WithMany(p => p.Ratings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Customer");

            entity.HasOne(d => d.Request).WithOne(p => p.Rating)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Request");

            entity.HasOne(d => d.Worker).WithMany(p => p.Ratings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Worker");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RefreshTokens_CreatedAt");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<RequestImage>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RequestImages_CreatedAt");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestImages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestImages_ServiceRequests");
        });

        modelBuilder.Entity<RequestStatusHistory>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RequestStatusHistories_CreatedAt");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.RequestStatusHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestStatusHistories_User");

            entity.HasOne(d => d.Request).WithMany(p => p.RequestStatusHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestStatusHistories_Request");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Services_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Services_IsActive");
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_ServiceRequests_CreatedAt");
            entity.Property(e => e.Status).HasDefaultValue("PENDING", "DF_ServiceRequests_Status");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Customer");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Service");

            entity.HasOne(d => d.Worker).WithMany(p => p.ServiceRequests).HasConstraintName("FK_ServiceRequests_Worker");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Users_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
        });

        modelBuilder.Entity<WorkerLocation>(entity =>
        {
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_WorkerLocations_UpdatedAt");

            entity.HasOne(d => d.Worker).WithOne(p => p.WorkerLocation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkerLocations_Worker");
        });

        modelBuilder.Entity<WorkerProfile>(entity =>
        {
            entity.Property(e => e.WorkerId).ValueGeneratedNever();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true, "DF_WorkerProfiles_IsAvailable");

            entity.HasOne(d => d.Worker).WithOne(p => p.WorkerProfile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkerProfiles_Users");
        });

        modelBuilder.Entity<WorkerService>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_WorkerServices_CreatedAt");

            entity.HasOne(d => d.Service).WithMany(p => p.WorkerServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkerServices_Service");

            entity.HasOne(d => d.Worker).WithMany(p => p.WorkerServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WorkerServices_Worker");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
