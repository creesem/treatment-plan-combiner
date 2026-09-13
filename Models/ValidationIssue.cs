namespace TreatmentPlanCombiner.Models;

public class ValidationIssue
{
    public IssueSeverity Severity { get; set; }
    public IssueType IssueType { get; set; }
    public string TreatmentPlanId { get; set; } = "";
    public string RecordId { get; set; } = "";
    public string Message { get; set; } = "";
}