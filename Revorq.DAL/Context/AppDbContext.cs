using Revorq.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Revorq.DAL.Context;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<InvitationToken> InvitationTokens => Set<InvitationToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Elevator> Elevators => Set<Elevator>();
    public DbSet<MaintenanceOrder> MaintenanceOrders => Set<MaintenanceOrder>();
    public DbSet<MaintenanceReport> MaintenanceReports => Set<MaintenanceReport>();
    public DbSet<UserBuildingAccess> UserBuildingAccesses => Set<UserBuildingAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Status).HasConversion<string>();
        });

        builder.Entity<InvitationToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Token).IsRequired().HasMaxLength(100);
            e.Property(t => t.Role).HasConversion<string>();

            e.HasOne(t => t.Company)
             .WithMany(c => c.InvitationTokens)
             .HasForeignKey(t => t.CompanyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(t => t.Token).IsUnique();
        });

        builder.Entity<AppUser>(e =>
        {
            e.Property(u => u.CompanyId).IsRequired();

            e.HasOne(u => u.Company)
             .WithMany(c => c.Members)
             .HasForeignKey(u => u.CompanyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(u => u.NormalizedUserName)
             .HasDatabaseName("UserNameIndex")
             .IsUnique(false);

            e.HasIndex(u => new { u.NormalizedUserName, u.CompanyId })
             .IsUnique();
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Token).IsRequired().HasMaxLength(200);
            e.HasIndex(t => t.Token).IsUnique();

            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Building>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(150);
            e.HasIndex(b => b.Name).IsUnique();
            e.Property(b => b.BuildingType).HasConversion<int>();
            e.Property(b => b.Status).HasConversion<string>();
            e.Property(b => b.Address).IsRequired().HasMaxLength(300);
            e.HasAlternateKey(b => b.Address);

            e.Property(b => b.FileUrls)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
             .HasColumnType("text");

            e.HasOne(b => b.Company)
             .WithMany(c => c.Buildings)
             .HasForeignKey(b => b.CompanyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Elevator>(e =>
        {
            e.HasKey(el => el.Id);
            e.Property(el => el.NumberInProject).IsRequired().HasMaxLength(50);
            e.Property(el => el.SerialNumber).IsRequired().HasMaxLength(100);
            e.HasIndex(el => el.SerialNumber).IsUnique();
            e.Property(el => el.Model).HasMaxLength(150);
            e.Property(el => el.ProductionCountry).HasMaxLength(100);
            e.Property(el => el.CustomerFullName).HasMaxLength(200);
            e.Property(el => el.CustomerPhoneNumber).HasMaxLength(50);
            e.Property(el => el.WarrantyType).HasConversion<int>();
            e.Property(el => el.Status).HasConversion<string>();
            e.Property(el => el.Priority).HasConversion<int>();

            e.HasOne(el => el.Building)
             .WithMany(b => b.Elevators)
             .HasForeignKey(el => el.BuildingId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MaintenanceOrder>(e =>
        {
            e.HasKey(o => o.Id);

            e.Property(o => o.MaintenanceType).HasConversion<string>();
            e.Property(o => o.Status).HasConversion<string>();
            e.Property(o => o.ShortDescription).HasMaxLength(1000);

            e.HasOne(o => o.Elevator)
             .WithMany(el => el.MaintenanceOrders)
             .HasForeignKey(o => o.ElevatorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(o => o.ImageUrls)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
             .HasColumnType("text");

            e.HasOne(o => o.AssignedEngineer)
             .WithMany(u => u.AssignedOrders)
             .HasForeignKey(o => o.AssignedEngineerId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Reporter)
             .WithMany()
             .HasForeignKey(o => o.ReporterId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(o => new { o.AssignedEngineerId, o.ScheduledDate });
        });

        builder.Entity<UserBuildingAccess>(e =>
        {
            e.HasKey(a => new { a.UserId, a.BuildingId });

            e.HasOne(a => a.User)
             .WithMany(u => u.BuildingAccesses)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Building)
             .WithMany(b => b.UserAccesses)
             .HasForeignKey(a => a.BuildingId)
             .OnDelete(DeleteBehavior.Cascade);
        });

builder.Entity<MaintenanceReport>(e =>
        {
            e.HasKey(r => r.OrderId);

            e.Property(r => r.ShortDescription).HasMaxLength(1000);

            e.Property(r => r.ImageUrls)
             .HasConversion(
                 v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                 v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
             .HasColumnType("text");

            e.HasOne(r => r.MaintenanceOrder)
             .WithOne(o => o.Report)
             .HasForeignKey<MaintenanceReport>(r => r.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
