using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class SettingsValidator
{
    public List<string> Validate(AppSettings settings)
    {
        var errors = new List<string>();

        ValidateFolder(settings.Folders.Input, nameof(FolderSettings.Input), errors);
        ValidateFolder(settings.Folders.Output, nameof(FolderSettings.Output), errors);
        ValidateFolder(settings.Folders.Logs, nameof(FolderSettings.Logs), errors);
        ValidateFolder(settings.Folders.DebugJson, nameof(FolderSettings.DebugJson), errors);

        ValidateFile(settings.Files.TreatmentPlans, nameof(FileSettings.TreatmentPlans), errors);
        ValidateFile(settings.Files.Problems, nameof(FileSettings.Problems), errors);
        ValidateFile(settings.Files.Goals, nameof(FileSettings.Goals), errors);
        ValidateFile(settings.Files.Objectives, nameof(FileSettings.Objectives), errors);
        ValidateFile(settings.Files.Interventions, nameof(FileSettings.Interventions), errors);
        ValidateFile(settings.Files.Activities, nameof(FileSettings.Activities), errors);

        ValidateFilePattern(
            settings.Files.OutputFilePattern,
            nameof(FileSettings.OutputFilePattern),
            errors);

        ValidateFilePattern(
            settings.Files.ValidationReportPattern,
            nameof(FileSettings.ValidationReportPattern),
            errors);

        ValidateFilePattern(
            settings.Files.ErrorLogPattern,
            nameof(FileSettings.ErrorLogPattern),
            errors);

        return errors;
    }

    private static void ValidateFolder(string? value, string propertyName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Folders.{propertyName} is missing or empty.");
        }
    }

    private static void ValidateFile(string? value, string propertyName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Files.{propertyName} is missing or empty.");
        }
    }

    private static void ValidateFilePattern(string? value, string propertyName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Files.{propertyName} is missing or empty.");
        }
        else if (!value.Contains("{timestamp}"))
        {
            errors.Add($"Files.{propertyName} must contain the '{{timestamp}}' placeholder.");
        }
    }
}