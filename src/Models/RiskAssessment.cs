namespace HealthCareChallenge.Models;

public class RiskAssessment
{
    public int RiskScore { get; set; }
    public string RiskStatus { get; set; } = "Unknown";
}