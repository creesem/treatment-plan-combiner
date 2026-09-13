using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class InputFileValidator
{
    public List<string> Validate(AppSettings settings)
    {
        var missingFiles = new List<string>();

        CheckFile(settings.Folders.Input, settings.Files.TreatmentPlans, missingFiles);
        CheckFile(settings.Folders.Input, settings.Files.Problems, missingFiles);
        CheckFile(settings.Folders.Input, settings.Files.Goals, missingFiles);
        CheckFile(settings.Folders.Input, settings.Files.Objectives, missingFiles);
        CheckFile(settings.Folders.Input, settings.Files.Interventions, missingFiles);
        CheckFile(settings.Folders.Input, settings.Files.Activities, missingFiles);

        return missingFiles;
    }

    private static void CheckFile(
        string inputFolder,
        string fileName,
        List<string> missingFiles)
    {
        var filePath = Path.Combine(inputFolder, fileName);

        if (!File.Exists(filePath))
        {
            missingFiles.Add(filePath);
        }
    }
}