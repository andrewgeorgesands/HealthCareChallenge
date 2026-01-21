using HealthCareChallenge.Models;
using HealthCareChallenge.Service.PatientService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Controllers;

[Authorize]
public class PatientController(IPatientService patientService, AppDbContext context, ILogger<PatientController> logger) : Controller
{
    [HttpGet]
    public IActionResult Vitals(int id)
    {
        var patient = patientService.GetPatientWithVitals(id);
        if (patient == null) return NotFound();

        logger.LogInformation("Accessing record for: {PatientFirstName} {PatientLastName}, {PatientSocialSecurityNumber}", patient.FirstName, patient.LastName, patient.SocialSecurityNumber);

        ViewBag.RiskAssessment = patientService.CalculateRiskAssessment(patient.Vitals);
        ViewBag.Inventory = patientService.GetInventory();

        return View(patient);
    }



    // POST: Patient/DispenseMedication
    // RACE CONDITION: Check (QuantityInStock > 0) and Act (decrement) are not atomic.
    // Two concurrent requests can both pass the check, both decrement, resulting in negative inventory.
    [HttpPost]
    public IActionResult DispenseMedication(int patientId, int inventoryId)
    {
        try
        {
            var success = patientService.DispenseMedication(patientId, inventoryId, out var drugName);

            if (success)
            {
                TempData["Message"] = $"Medication {drugName} Dispensed";
            }
            else
            {
                TempData["Error"] = "Medication not found or Out of Stock";
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Another user dispensed this medication. Please try again.";
        }

        return RedirectToAction(nameof(Vitals), new { id = patientId });
    }

    // POST: Patient/Delete/5
    // The patient was only deleted and cascade behaviour is restricted.
    // added a deletion process of vitals too here.
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var deleted = patientService.DeletePatientWithVitals(id);

        if (!deleted) return NotFound();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AllVitals()
    {
        var vitals = context.Vitals.ToList();
        var patientVitalsIds = context.Patients.Select(p => p.VitalsId).ToList();

        ViewBag.PatientVitalsIds = patientVitalsIds;

        return View(vitals);
    }

    [HttpGet]
    public IActionResult DispensedHistory()
    {
        ViewBag.Patients = patientService.GetPatientNames();
        ViewBag.Inventory = patientService.GetMedicationNames();

        return View(patientService.GetDispensedHistory());
    }
}

