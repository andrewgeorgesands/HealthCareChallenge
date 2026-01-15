using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Vitals> Vitals { get; set; }
    public DbSet<MedicationInventory> Inventory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Vitals)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vitals>().HasData(
            new Vitals { Id = 1, HeartRate = 110, SystolicBP = 140, Temperature = 39.5 },
            new Vitals { Id = 2, HeartRate = 70, SystolicBP = 120, Temperature = 36.6 }
        );

        modelBuilder.Entity<Patient>().HasData(
            new { Id = 1, FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789", VitalsId = 1 },
            new { Id = 2, FirstName = "Jane", LastName = "Smith", SocialSecurityNumber = "987-65-4321", VitalsId = 2 }
        );

        modelBuilder.Entity<MedicationInventory>().HasData(
            new MedicationInventory { Id = 1, DrugName = "Morphine (10mg Vial)", QuantityInStock = 10 }
        );
    }
}
