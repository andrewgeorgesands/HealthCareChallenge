namespace HealthCareChallenge.Models;

public class Vitals
{
    public int Id { get; set; }
    public int HeartRate { get; set; } // BPM
    public int SystolicBP { get; set; } // Blood Pressure
    public double Temperature { get; set; } // Celsius
}