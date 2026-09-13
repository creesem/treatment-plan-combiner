using System.Globalization;
using CsvHelper;
using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class ValidationReportWriter
{
    public void Write(
        string filePath,
        List<ValidationIssue> issues,
        int treatmentPlanCount,
        int problemCount,
        int goalCount,
        int objectiveCount,
        int interventionCount,
        int activityCount,
        int outputRecordCount)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var reportRows = new List<ValidationReportRow>();

        reportRows.Add(new ValidationReportRow
        {
            RunTimestamp = timestamp,
            Status = issues.Any(x => x.Severity == IssueSeverity.Error)
                ? "CompletedWithErrors"
                : "Success",
            TreatmentPlanCount = treatmentPlanCount,
            ProblemCount = problemCount,
            GoalCount = goalCount,
            ObjectiveCount = objectiveCount,
            InterventionCount = interventionCount,
            ActivityCount = activityCount,
            OutputRecordCount = outputRecordCount,
            IssueSeverity = string.Empty,
            IssueType = string.Empty,
            TreatmentPlanId = string.Empty,
            RecordId = string.Empty,
            Message = "Run summary"
        });

        foreach (var issue in issues)
        {
            reportRows.Add(new ValidationReportRow
            {
                RunTimestamp = timestamp,
                Status = "Issue",
                TreatmentPlanCount = treatmentPlanCount,
                ProblemCount = problemCount,
                GoalCount = goalCount,
                ObjectiveCount = objectiveCount,
                InterventionCount = interventionCount,
                ActivityCount = activityCount,
                OutputRecordCount = outputRecordCount,
                IssueSeverity = issue.Severity.ToString(),
                IssueType = issue.IssueType.ToString(),
                TreatmentPlanId = issue.TreatmentPlanId,
                RecordId = issue.RecordId,
                Message = issue.Message
            });
        }

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(reportRows);
    }
}