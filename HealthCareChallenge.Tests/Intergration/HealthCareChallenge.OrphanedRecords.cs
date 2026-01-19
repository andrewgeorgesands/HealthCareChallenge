using HealthCareChallenge.Controllers;
using HealthCareChallenge.Models;
using HealthCareChallenge.Service.PatientService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace HealthCareChallenge.Tests.Intergration;

public class OrphanRecordTests
{
    [Fact]
    public void DeletePatient_ShouldAlsoDeleteVitals()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // setup
        using (var context = new AppDbContext(options))
        {
            context.Vitals.Add(new Vitals { Id = 1, HeartRate = 70, SystolicBP = 120, Temperature = 36.6 });
            context.Patients.Add(new Patient { Id = 1, FirstName = "Test", LastName = "Patient", SocialSecurityNumber = "000-00-0000", VitalsId = 1 });
            context.SaveChanges();
        }

        // Act
        using (var context = new AppDbContext(options))
        {
            var logger = new LoggerFactory().CreateLogger<PatientController>();
            var patientService = new PatientService(context);
            var controller = new PatientController(patientService, context, logger);

            controller.Delete(1);
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var patientExists = context.Patients.Any(p => p.Id == 1);
            var vitalsExists = context.Vitals.Any(v => v.Id == 1);

            Assert.False(patientExists);
            Assert.False(vitalsExists); 
        }
    }
}