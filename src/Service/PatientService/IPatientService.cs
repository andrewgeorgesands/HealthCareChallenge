using HealthCareChallenge.Models;

namespace HealthCareChallenge.Service.PatientService;

public interface IPatientService
{
    Patient? GetPatientWithVitals(int id);
    List<MedicationInventory> GetInventory();
    RiskAssessment CalculateRiskAssessment(Vitals? vitals);

    void UpdateRiskAssessment(Patient patient);
    public bool DeletePatientWithVitals(int id);

    bool DispenseMedication(int patientId, int inventoryId, out string? drugName);

    List<DispensedMedication> GetDispensedHistory();
    Dictionary<int, string> GetPatientNames();
    Dictionary<int, string> GetMedicationNames();
}