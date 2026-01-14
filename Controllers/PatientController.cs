using HealthCareChallenge.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareChallenge.Controllers;

// looks like this functionality is not under authorization, I wonder if I can access it without login
public class PatientController(AppDbContext context, ILogger<PatientController> logger) : Controller
{
    // GET: Patient/Vitals/5
    public IActionResult Vitals(int id)
    {
        var patient = context.Patients.Find(id);
        if (patient == null) return NotFound();

        // we should never log personal identifiable data to the logs.  If logging is necessary, the we must use a different unique identifier such as patient id.
        logger.LogInformation("Accessing record for: {PatientFirstName} {PatientLastName}, SSN: {PatientSocialSecurityNumber}", patient.FirstName, patient.LastName, patient.SocialSecurityNumber);

        var riskScore = 0;
        var status = "Stable";

        if (patient.HeartRate > 90) riskScore += 1;
        if (patient.HeartRate > 110) riskScore += 3; // Severe
        if (patient.SystolicBP < 100) riskScore += 2;
        if (patient.Temperature > 38.0) riskScore += 1;

        if (riskScore >= 5) status = "CRITICAL - SEPSIS ALERT";
        else if (riskScore >= 3) status = "Warning - Monitor Closely";

        ViewBag.RiskStatus = status;
        ViewBag.RiskScore = riskScore;

        // Fetch inventory data
        ViewBag.Inventory = context.Inventory.ToList();

        return View(patient);
    }

    // POST: Patient/DispenseMedication
    [HttpPost]
    public IActionResult DispenseMedication(int patientId, int inventoryId)
    {
        var meds = context.Inventory.Find(inventoryId);

        // need to fix this race condition.  If two doctors (or just two tabs) call the API at the same time, then
        // the quantity in stock will not update properly and cause inventory to be wrong.
        if (meds is { QuantityInStock: > 0 })
        {
            // Simulate a network delay in medication delivery
            Thread.Sleep(2000);

            meds.QuantityInStock--;
            context.SaveChanges();

            TempData["Message"] = $"Medication {meds.DrugName} Dispensed";
        }
        else
        {
            TempData["Error"] = meds == null ? "Medication not found" : "Out of Stock";
        }

        return RedirectToAction(nameof(Vitals), new { id = patientId });
    }
}