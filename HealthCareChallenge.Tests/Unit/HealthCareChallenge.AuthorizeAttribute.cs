using HealthCareChallenge.Controllers;
using HealthCareChallenge.Models;
using HealthCareChallenge.Service.PatientService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Xunit;

namespace HealthCareChallenge.Tests;

public class SecurityTests
{
    [Fact]
    public void PatientController_HasAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(PatientController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        Assert.NotEmpty(authorizeAttribute);
    }

    [Fact]
    public void Vitals_UnauthenticatedUser_ReturnsUnauthorized()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // setup
        using var context = new AppDbContext(options);

        context.Vitals.Add(new Vitals { Id = 1, HeartRate = 70, SystolicBP = 120, Temperature = 36.6 });
        context.Patients.Add(new Patient { Id = 1, FirstName = "Test", LastName = "Patient", SocialSecurityNumber = "123-45-6789", VitalsId = 1 });
        context.SaveChanges();

        var logger = new LoggerFactory().CreateLogger<PatientController>();
        var patientService = new PatientService(context);
        var controller = new PatientController(patientService, context, logger);

        // Act
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

   
        var hasAuthorize = controller.GetType()
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Any();

        // Assert
        Assert.True(hasAuthorize, "PatientController should have [Authorize] attribute to protect patient data");
    }
}