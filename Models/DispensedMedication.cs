namespace HealthCareChallenge.Models;

public class DispensedMedication
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int InventoryId { get; set; }
    public DateTime DispensedAt { get; set; }
}
