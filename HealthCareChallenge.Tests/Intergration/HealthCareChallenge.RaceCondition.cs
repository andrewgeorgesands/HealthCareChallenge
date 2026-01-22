using HealthCareChallenge.Models;
using HealthCareChallenge.Service.PatientService;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthCareChallenge.Tests.Intergration;

public class RaceConditionTests
{
    [Fact]
    public void DispenseMedication_ConcurrentRequests_ThrowsConcurrencyException()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        // Setup
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();
            context.Inventory.Add(new MedicationInventory { Id = 100, DrugName = "TestDrug", QuantityInStock = 1 });
            context.Patients.Add(new Patient { Id = 100, FirstName = "Test", LastName = "Patient", SocialSecurityNumber = "000-00-0000", VitalsId = 100 });
            context.Vitals.Add(new Vitals { Id = 100, HeartRate = 70, SystolicBP = 120, Temperature = 36.6 });
            context.SaveChanges();
        }

        using var context1 = new AppDbContext(options);
        using var context2 = new AppDbContext(options);

        // This simulates two requests hitting the server at the same instant
        var preload1 = context1.Inventory.Find(100); 
        var preload2 = context2.Inventory.Find(100); 

        var service1 = new PatientService(context1);
        var service2 = new PatientService(context2);

        //Act

        // User 1 dispenses (succeeds, DB now has QuantityInStock = 0)
        var result1 = service1.DispenseMedication(100, 100, out _);

        // User 2 tries to dispense
        // context2 still has stale data (QuantityInStock = 1)
        Assert.Throws<DbUpdateConcurrencyException>(() =>
            service2.DispenseMedication(100, 100, out _));

        // Assert

        // Verify stock is 0, not negative
        using var finalContext = new AppDbContext(options);
        var finalStock = finalContext.Inventory.Find(100)!.QuantityInStock;
        Assert.Equal(0, finalStock);

        connection.Close();
    }
}