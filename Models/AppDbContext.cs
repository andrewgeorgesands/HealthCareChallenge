using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<MedicationInventory> Inventory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>().HasData(
            new Patient { Id = 1, FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789", HeartRate = 110, SystolicBP = 140, Temperature = 39.5 },
            new Patient { Id = 2, FirstName = "Jane", LastName = "Smith", SocialSecurityNumber = "987-65-4321", HeartRate = 70, SystolicBP = 120, Temperature = 36.6 }
        );

        modelBuilder.Entity<MedicationInventory>().HasData(
            new MedicationInventory { Id = 1, DrugName = "Morphine (10mg Vial)", QuantityInStock = 10 }
        );
    }
}
