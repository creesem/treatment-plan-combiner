namespace TreatmentPlanCombiner.Models;

public class Objective
{
    public string ObjectiveId { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Status { get; set; } = "";
    public List<Intervention> Interventions { get; set; } = new();
}