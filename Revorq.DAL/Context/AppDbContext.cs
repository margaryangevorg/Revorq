using Revorq.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Context;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<InvitationToken> InvitationTokens => Set<InvitationToken>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Elevator> Elevators => Set<Elevator>();
    public DbSet<MaintenanceOrder> MaintenanceOrders => Set<MaintenanceOrder>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
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
        });

        builder.Entity<Building>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(150);
            e.Property(b => b.Address).IsRequired().HasMaxLength(300);
        });

        builder.Entity<Elevator>(e =>
        {
            e.HasKey(el => el.Id);
            e.Property(el => el.Label).IsRequired().HasMaxLength(20);
            e.Property(el => el.SerialNumber).HasMaxLength(100);

            e.HasOne(el => el.Building)
             .WithMany(b => b.Elevators)
             .HasForeignKey(el => el.BuildingId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MaintenanceOrder>(e =>
        {
            e.HasKey(o => o.Id);

            e.Property(o => o.MaintenanceType).HasConversion<string>();
            e.Property(o => o.ShortDescription).HasMaxLength(1000);

            e.HasOne(o => o.Elevator)
             .WithMany(el => el.MaintenanceOrders)
             .HasForeignKey(o => o.ElevatorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.AssignedEngineer)
             .WithMany(u => u.AssignedOrders)
             .HasForeignKey(o => o.AssignedEngineerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(o => new { o.AssignedEngineerId, o.ScheduledDate });
        });
    }
}
