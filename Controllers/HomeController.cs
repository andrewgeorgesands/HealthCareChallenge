using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using HealthCareChallenge.Models;
using Microsoft.AspNetCore.Authorization;

namespace HealthCareChallenge.Controllers;

[Authorize]
public class HomeController(AppDbContext context, ILogger<HomeController> logger) : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        var patients = User.Identity?.IsAuthenticated == true 
            ? context.Patients.ToList() 
            : [];
            
        return View(patients);
    } 

    [HttpPost]
    public async Task<IActionResult> ResetPatients()
    {
        // Remove all patients
        context.Patients.RemoveRange(context.Patients);
        // Also remove all vitals to have a clean slate if resetting
        context.Vitals.RemoveRange(context.Vitals);
        await context.SaveChangesAsync();

        // Re-seed vitals first
        var vitals1 = new Vitals { HeartRate = 110, SystolicBP = 140, Temperature = 39.5 };
        var vitals2 = new Vitals { HeartRate = 70, SystolicBP = 120, Temperature = 36.6 };
        context.Vitals.AddRange(vitals1, vitals2);
        await context.SaveChangesAsync();

        // Re-seed patients
        context.Patients.AddRange(
            new Patient { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789", VitalsId = vitals1.Id },
            new Patient { FirstName = "Jane", LastName = "Smith", SocialSecurityNumber = "987-65-4321", VitalsId = vitals2.Id }
        );

        await context.SaveChangesAsync();
        TempData["Message"] = "Patients have been re-seeded.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

