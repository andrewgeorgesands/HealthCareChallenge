using HealthCareChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Controllers;

public class PatientController(AppDbContext context, ILogger<PatientController> logger) : Controller
{
    // GET: Patient/Vitals/5
    public IActionResult Vitals(int id)
    {
        var patient = context.Patients
            .Include(p => p.Vitals)
            .FirstOrDefault(p => p.Id == id);
        if (patient == null) return NotFound();

        logger.LogInformation("Accessing record for: {PatientFirstName} {PatientLastName}, SSN: {PatientSocialSecurityNumber}", patient.FirstName, patient.LastName, patient.SocialSecurityNumber);

        var riskScore = 0;
        var status = "Stable";

        if (patient.Vitals.HeartRate > 90) riskScore += 1;
        if (patient.Vitals.HeartRate > 110) riskScore += 3; // Severe
        if (patient.Vitals.SystolicBP < 100) riskScore += 2;
        if (patient.Vitals.Temperature > 38.0) riskScore += 1;

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

        if (meds is { QuantityInStock: > 0 })
        {
            meds.QuantityInStock--;
            
            context.DispensedMedications.Add(new DispensedMedication
            {
                PatientId = patientId,
                InventoryId = inventoryId,
                DispensedAt = DateTime.UtcNow
            });
            
            context.SaveChanges();

            TempData["Message"] = $"Medication {meds.DrugName} Dispensed";
        }
        else
        {
            TempData["Error"] = meds == null ? "Medication not found" : "Out of Stock";
        }

        return RedirectToAction(nameof(Vitals), new { id = patientId });
    }

    // POST: Patient/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var patient = context.Patients.Find(id);
        if (patient == null) return NotFound();

        context.Patients.Remove(patient);
        context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    // GET: Patient/AllVitals
    public IActionResult AllVitals()
    {
        var vitals = context.Vitals.ToList();
        var patientVitalsIds = context.Patients.Select(p => p.VitalsId).ToList();

        ViewBag.PatientVitalsIds = patientVitalsIds;

        return View(vitals);
    }

    // GET: Patient/DispensedHistory
    public IActionResult DispensedHistory()
    {
        var history = context.DispensedMedications
            .OrderByDescending(d => d.DispensedAt)
            .ToList();

        // Simple way to get names for display without full joins for now, 
        // given the challenge context
        ViewBag.Patients = context.Patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");
        ViewBag.Inventory = context.Inventory.ToDictionary(i => i.Id, i => i.DrugName);

        return View(history);
    }
}
