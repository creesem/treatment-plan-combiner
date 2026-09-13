namespace TreatmentPlanCombiner.Models;

public class ActivityRecord
{
    public string Id { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string PlanFieldActActivityId { get; set; } = "";
    public string PlanFieldActActivity { get; set; } = "";
    public string PlanFieldActStartDate { get; set; } = "";
    public string PlanFieldActDuration { get; set; } = "";
    public string PlanFieldActFrequencyFrom { get; set; } = "";
    public string PlanFieldActFrequencyTo { get; set; } = "";
    public string PlanFieldActFrequencyPeriod { get; set; } = "";
    public string PlanFieldActAmount { get; set; } = "";
}