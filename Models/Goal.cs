namespace TreatmentPlanCombiner.Models;

public class Goal
{
    public string GoalId { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Status { get; set; } = "";
    public List<Objective> Objectives { get; set; } = new();
}