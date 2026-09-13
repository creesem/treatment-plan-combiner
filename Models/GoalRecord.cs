namespace TreatmentPlanCombiner.Models;

public class GoalRecord
{
    public string Id { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string GoalId { get; set; } = "";
    public string ProblemId { get; set; } = "";
    public string PlanFieldGoalNarrative { get; set; } = "";
    public string PlanFieldGoalPriority { get; set; } = "";
    public string PlanFieldGoalStatus { get; set; } = "";
    public string PlanFieldProgressTowardsGoal { get; set; } = "";
    public string PlanFieldProgressGoalRating { get; set; } = "";
    public string PlanFieldProgressGoalCompletedDate { get; set; } = "";
}