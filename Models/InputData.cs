namespace TreatmentPlanCombiner.Models;

public class InputData
{
    public List<TreatmentPlanRecord> TreatmentPlans { get; set; } = new();
    public List<ProblemRecord> Problems { get; set; } = new();
    public List<GoalRecord> Goals { get; set; } = new();
    public List<ObjectiveRecord> Objectives { get; set; } = new();
    public List<InterventionRecord> Interventions { get; set; } = new();
    public List<ActivityRecord> Activities { get; set; } = new();
}