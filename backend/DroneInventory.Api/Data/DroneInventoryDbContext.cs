using DroneInventory.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DroneInventory.Api.Data;

public class DroneInventoryDbContext(DbContextOptions<DroneInventoryDbContext> options) : DbContext(options)
{
    public DbSet<Drone> Drones => Set<Drone>();

    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<LoanItem> LoanItems => Set<LoanItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Drone>(entity =>
        {
            entity.Property(drone => drone.Name).IsRequired().HasMaxLength(200);
            entity.Property(drone => drone.Description).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<StudentGroup>(entity =>
        {
            entity.Property(group => group.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasOne(loan => loan.Drone)
                .WithMany()
                .HasForeignKey(loan => loan.DroneId);

            entity.HasOne(loan => loan.StudentGroup)
                .WithMany(group => group.Loans)
                .HasForeignKey(loan => loan.StudentGroupId);

            entity.HasMany(loan => loan.Items)
                .WithOne(item => item.Loan)
                .HasForeignKey(item => item.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // A drone has at most one active loan: unique on DroneId, only for loans without a return date.
            entity.HasIndex(loan => loan.DroneId)
                .IsUnique()
                .HasFilter("\"ReturnedOn\" IS NULL");
        });

        modelBuilder.Entity<LoanItem>(entity =>
        {
            entity.Property(item => item.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            // At most one accessory entry per type per loan.
            entity.HasIndex(item => new { item.LoanId, item.Type }).IsUnique();

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_LoanItems_Quantity_NonNegative",
                "\"Quantity\" >= 0"));
        });
    }
}
