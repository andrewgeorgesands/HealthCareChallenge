using HealthCareChallenge.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthCareChallenge.Tests.Unit;

public class RaceConditionTests
{
    [Fact]
    public void DispenseMedication_ConcurrentRequests_ThrowsConcurrencyException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        //setup
        using (var context = new AppDbContext(options))
        {
            context.Inventory.Add(new MedicationInventory { Id = 1, DrugName = "TestDrug", QuantityInStock = 1 });
            context.Patients.Add(new Patient { Id = 1, FirstName = "Test", LastName = "Patient", SocialSecurityNumber = "000-00-0000", VitalsId = 1 });
            context.Vitals.Add(new Vitals { Id = 1, HeartRate = 70, SystolicBP = 120, Temperature = 36.6 });
            context.SaveChanges();
        }

        // act
        var context1 = new AppDbContext(options);
        var context2 = new AppDbContext(options);

        var meds1 = context1.Inventory.Find(1);
        var meds2 = context2.Inventory.Find(1);

        meds1!.QuantityInStock--;
        meds2!.QuantityInStock--;

        context1.SaveChanges();

        // assert
        Assert.Throws<DbUpdateConcurrencyException>(() => context2.SaveChanges());

        // Verify inventory didn't go negative
        using var finalContext = new AppDbContext(options);
        var finalStock = finalContext.Inventory.Find(1)!.QuantityInStock;
        Assert.Equal(0, finalStock);
    }
}