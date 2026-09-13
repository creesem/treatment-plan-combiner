using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class FolderInitializer
{
    public void EnsureFoldersExist(AppSettings settings)
    {
        Directory.CreateDirectory(settings.Folders.Input);
        Directory.CreateDirectory(settings.Folders.Output);
        Directory.CreateDirectory(settings.Folders.Logs);
        Directory.CreateDirectory(settings.Folders.DebugJson);
    }
}