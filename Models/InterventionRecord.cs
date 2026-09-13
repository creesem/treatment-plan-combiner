namespace TreatmentPlanCombiner.Models;

public class InterventionRecord
{
    public string Id { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string InterventionId { get; set; } = "";
    public string ObjectiveId { get; set; } = "";
    public string PlanFieldIntvNarrative { get; set; } = "";
    public string PlanFieldIntvPriority { get; set; } = "";
    public string PlanFieldIntvStatus { get; set; } = "";
    public string PlanFieldIntvFrequencyFrom { get; set; } = "";
    public string PlanFieldIntvFrequencyTo { get; set; } = "";
    public string PlanFieldIntvFrequencyPeriod { get; set; } = "";
    public string PlanFieldIntvResponsibleParty { get; set; } = "";
    public string PlanFieldIntvStartDate { get; set; } = "";
    public string PlanFieldIntvTargetDate { get; set; } = "";
    public string PlanFieldIntvStaffForReview { get; set; } = "";
    public string PlanFieldIntvReviewDate { get; set; } = "";
    public string PlanFieldIntvCompletedDate { get; set; } = "";
}