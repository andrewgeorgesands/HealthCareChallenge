namespace HealthCareChallenge.Models;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
        
    public string SocialSecurityNumber { get; set; } 
    
    public int VitalsId { get; set; }
    public Vitals Vitals { get; set; }

    public int? PatientRiskId { get; set; }
    public PatientRisk PatientRisk { get; set; }
}