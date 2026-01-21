using HealthCareChallenge.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Service.PatientService;

public class PatientService(AppDbContext context) : IPatientService
{
    public Patient? GetPatientWithVitals(int id)
    {
        return context.Patients
            .Include(p => p.Vitals)
            .Include(p => p.PatientRisk)
            .FirstOrDefault(p => p.Id == id);
    }

    public List<MedicationInventory> GetInventory()
    {
        return context.Inventory.ToList();
    }

    public RiskAssessment CalculateRiskAssessment(Vitals? vitals)
    {
        if (vitals == null) return new RiskAssessment { RiskScore = 0, RiskStatus = "Unknown" };

        var riskScore = 0;

        if (vitals.HeartRate > 90) riskScore += 1;
        if (vitals.HeartRate > 110) riskScore += 3;
        if (vitals.SystolicBP < 100) riskScore += 2;
        if (vitals.Temperature > 38.0) riskScore += 1;
        if (vitals.Temperature > 40.0) riskScore += 3;

        var status = riskScore >= 5 ? "CRITICAL - SEPSIS ALERT"
                   : riskScore >= 3 ? "Warning - Monitor Closely"
                   : "Stable";

        return new RiskAssessment { RiskScore = riskScore, RiskStatus = status };
    }

    public void UpdateRiskAssessment(Patient patient)
    {
        if (patient.Vitals == null) return;

        // Project vitals WITHOUT mutating the original (fixes idempotency issue)
        var projectedVitals = new Vitals
        {
            HeartRate = (int)Math.Round(patient.Vitals.HeartRate * 1.004),
            SystolicBP = (int)Math.Round(patient.Vitals.SystolicBP * 0.934),
            Temperature = Math.Round(patient.Vitals.Temperature * 1.005, 2)
        };
        var risk = CalculateRiskAssessment(projectedVitals);

        if (patient.PatientRisk == null)
        {
            patient.PatientRisk = new PatientRisk();
        }

        patient.PatientRisk.RiskScore = risk.RiskScore;
        patient.PatientRisk.RiskStatus = risk.RiskStatus;
    }

    public bool DeletePatientWithVitals(int id)
    {
        var patient = context.Patients
            .Include(p => p.Vitals)
            .FirstOrDefault(p => p.Id == id);

        if (patient == null) return false;

        var vitals = patient.Vitals;

        context.Patients.Remove(patient);

        if (vitals != null)
        {
            context.Vitals.Remove(vitals);
        }

        context.SaveChanges();
        return true;
    }

    public bool DispenseMedication(int patientId, int inventoryId, out string? drugName)
    {
        drugName = null;

        var meds = context.Inventory.Find(inventoryId);

        if (meds is not { QuantityInStock: > 0 })
        {
            return false;
        }

        drugName = meds.DrugName;
        meds.QuantityInStock--;

        context.DispensedMedications.Add(new DispensedMedication
        {
            PatientId = patientId,
            InventoryId = inventoryId,
            DispensedAt = DateTime.UtcNow
        });

        if (meds.DrugName.Contains("Morphine", StringComparison.OrdinalIgnoreCase))
        {
            var patient = context.Patients
                .Include(p => p.Vitals)
                .Include(p => p.PatientRisk)
                .FirstOrDefault(p => p.Id == patientId);

            if (patient?.Vitals != null)
            {
                UpdateRiskAssessment(patient);
            }
        }

        context.SaveChanges();
        return true;
    }

    public List<DispensedMedication> GetDispensedHistory()
    {
        return context.DispensedMedications
            .OrderByDescending(d => d.DispensedAt)
            .ToList();
    }

    public Dictionary<int, string> GetPatientNames()
    {
        return context.Patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");
    }

    public Dictionary<int, string> GetMedicationNames()
    {
        return context.Inventory.ToDictionary(i => i.Id, i => i.DrugName);
    }
}