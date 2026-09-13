namespace TreatmentPlanCombiner.Models;

public class AppSettings
{
    public FolderSettings Folders { get; set; } = new();
    public FileSettings Files { get; set; } = new();
}

public class FolderSettings
{
    public string Input { get; set; } = "Input/sample";
    public string Output { get; set; } = "Output";
    public string Logs { get; set; } = "Logs";
    public string DebugJson { get; set; } = "Output/DebugJson";
}

public class FileSettings
{
    public string TreatmentPlans { get; set; } = "treatment_plans.csv";
    public string Problems { get; set; } = "problems.csv";
    public string Goals { get; set; } = "goals.csv";
    public string Objectives { get; set; } = "objectives.csv";
    public string Interventions { get; set; } = "interventions.csv";
    public string Activities { get; set; } = "activities.csv";
    public string OutputFilePattern { get; set; } = "treatment_plans_{timestamp}.csv";
    public string ValidationReportPattern { get; set; } = "validation_report_{timestamp}.csv";
    public string ErrorLogPattern { get; set; } = "application_error_{timestamp}.log";
}