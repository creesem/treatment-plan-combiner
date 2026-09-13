namespace TreatmentPlanCombiner.Models;

public class ObjectiveRecord
{
    public string Id { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string ObjectiveId { get; set; } = "";
    public string GoalId { get; set; } = "";
    public string PlanFieldObjectiveNarrative { get; set; } = "";
    public string PlanFieldObjectivePriority { get; set; } = "";
    public string PlanFieldObjectiveStatus { get; set; } = "";
    public string PlanFieldObjectiveFrequencyFrom { get; set; } = "";
    public string PlanFieldObjectiveFrequencyTo { get; set; } = "";
    public string PlanFieldObjectiveFrequencyPeriod { get; set; } = "";
    public string PlanFieldObjectiveRating { get; set; } = "";
    public string PlanFieldObjectiveResponsibleParty { get; set; } = "";
    public string PlanFieldObjectiveClientProgramId { get; set; } = "";
    public string PlanFieldObjectiveStartDate { get; set; } = "";
    public string PlanFieldObjectiveTargetDate { get; set; } = "";
    public string PlanFieldObjectiveStaffForReview { get; set; } = "";
    public string PlanFieldObjectiveReviewDate { get; set; } = "";
    public string PlanFieldObjectiveProgressTowardObjective { get; set; } = "";
    public string PlanFieldObjectiveCompletedDate { get; set; } = "";
}