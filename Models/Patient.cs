namespace HealthCareChallenge.Models;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
        
    public string SocialSecurityNumber { get; set; } 
        
    public int HeartRate { get; set; } // BPM
    public int SystolicBP { get; set; } // Blood Pressure
    public double Temperature { get; set; } // Celsius
}