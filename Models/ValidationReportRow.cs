namespace TreatmentPlanCombiner.Models;

public class ValidationReportRow
{
    public string RunTimestamp { get; set; } = "";
    public string Status { get; set; } = "";
    public int TreatmentPlanCount { get; set; }
    public int ProblemCount { get; set; }
    public int GoalCount { get; set; }
    public int ObjectiveCount { get; set; }
    public int InterventionCount { get; set; }
    public int ActivityCount { get; set; }
    public int OutputRecordCount { get; set; }
    public string IssueSeverity { get; set; } = "";
    public string IssueType { get; set; } = "";
    public string TreatmentPlanId { get; set; } = "";
    public string RecordId { get; set; } = "";
    public string Message { get; set; } = "";
}