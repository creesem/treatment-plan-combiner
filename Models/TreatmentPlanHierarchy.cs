namespace TreatmentPlanCombiner.Models;

public class TreatmentPlanHierarchy
{
    public string TreatmentPlanId { get; set; } = "";
    public string PatientId { get; set; } = "";
    public List<Problem> Problems { get; set; } = new();
}