namespace TreatmentPlanCombiner.Models;

public class Problem
{
    public string ProblemId { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Status { get; set; } = "";
    public List<Goal> Goals { get; set; } = new();
}