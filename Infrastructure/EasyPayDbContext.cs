using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class EasyPayDbContext : DbContext
{


    public DbSet<Client> Clients { get; set; }
    public DbSet<Technician> Technicians { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    
    public EasyPayDbContext(DbContextOptions<EasyPayDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Client>()
        .HasKey(c => c.Id);

    modelBuilder.Entity<Client>()
        .Property(c => c.FirstName)
        .HasMaxLength(100)
        .IsRequired();

    modelBuilder.Entity<Client>()
        .Property(c => c.LastName)
        .HasMaxLength(100)
        .IsRequired();
    

    modelBuilder.Entity<Technician>()
        .HasKey(t => t.Id);

    modelBuilder.Entity<Technician>()
        .Property(t => t.FirstName)
        .HasMaxLength(100)
        .IsRequired();

    modelBuilder.Entity<Technician>()
        .Property(t => t.LastName)
        .HasMaxLength(100)
        .IsRequired();

    modelBuilder.Entity<Technician>()
        .HasIndex(t => new { t.FirstName, t.LastName })
        .IsUnique();
    

    modelBuilder.Entity<WorkOrder>()
        .HasKey(w => w.Id);

    modelBuilder.Entity<WorkOrder>()
        .Property(w => w.Information)
        .HasMaxLength(2000)
        .IsRequired();

    modelBuilder.Entity<WorkOrder>()
        .HasOne(w => w.Client)
        .WithMany()
        .HasForeignKey(w => w.ClientId)
        .OnDelete(DeleteBehavior.Restrict);


    modelBuilder.Entity<WorkOrder>()
        .HasOne(w => w.Technician)
        .WithMany()
        .HasForeignKey(w => w.TechnicianId)
        .OnDelete(DeleteBehavior.Restrict);


    modelBuilder.Entity<WorkOrder>()
        .HasIndex(w => w.TechnicianId);

    modelBuilder.Entity<WorkOrder>()
        .HasIndex(w => w.ClientId);
    }
}