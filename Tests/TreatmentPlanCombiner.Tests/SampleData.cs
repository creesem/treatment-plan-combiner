using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Tests;

internal sealed class TestDirectory : IDisposable
{
    public TestDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "tpc-" + Guid.NewGuid().ToString("N"));
    }

    public string Path { get; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup only.
        }
    }
}

internal static class SampleData
{
    public static AppSettings Settings(string inputFolder, string root)
    {
        return new AppSettings
        {
            Folders = new FolderSettings
            {
                Input = inputFolder,
                Output = System.IO.Path.Combine(root, "Output"),
                Logs = System.IO.Path.Combine(root, "Logs"),
                DebugJson = System.IO.Path.Combine(root, "Output", "DebugJson")
            }
        };
    }

    public static void WriteFile(string folder, string fileName, string content)
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(System.IO.Path.Combine(folder, fileName), content);
    }

    public static void WriteValidInputFiles(string inputFolder)
    {
        WriteFile(inputFolder, "treatment_plans.csv",
            "Id,Patient Id\n" +
            "TP-1,PAT-1\n" +
            "TP-2,PAT-2\n");

        WriteFile(inputFolder, "problems.csv",
            "Id,Patient Id,Problem Id,Plan Field Problem Narrative\n" +
            "TP-1,PAT-1,PRB-1,Problem narrative one\n" +
            "TP-2,PAT-2,PRB-2,Problem narrative two\n");

        WriteFile(inputFolder, "goals.csv",
            "Id,Patient Id,Goal Id,Problem Id,Plan Field Goal Narrative\n" +
            "TP-1,PAT-1,G-1,PRB-1,Goal narrative one\n" +
            "TP-2,PAT-2,G-2,PRB-2,Goal narrative two\n");

        WriteFile(inputFolder, "objectives.csv",
            "Id,Patient Id,Objective Id,Goal Id,Plan Field Objective Narrative\n" +
            "TP-1,PAT-1,OBJ-1,G-1,Objective narrative one\n" +
            "TP-2,PAT-2,OBJ-2,G-2,Objective narrative two\n");

        WriteFile(inputFolder, "interventions.csv",
            "Id,Patient Id,Intervention Id,Objective Id,Plan Field Intv Narrative\n" +
            "TP-1,PAT-1,INT-1,OBJ-1,Intervention narrative one\n" +
            "TP-2,PAT-2,INT-2,OBJ-2,Intervention narrative two\n");

        WriteFile(inputFolder, "activities.csv",
            "Id,Patient Id,Plan Field Act Activity Id,Plan Field Act Activity\n" +
            "TP-1,PAT-1,ACT-1,Group Counseling\n" +
            "TP-2,PAT-2,ACT-2,Individual Counseling\n");
    }

    public static void WriteOrphanInputFiles(string inputFolder)
    {
        WriteValidInputFiles(inputFolder);

        WriteFile(inputFolder, "problems.csv",
            "Id,Patient Id,Problem Id,Plan Field Problem Narrative\n" +
            "TP-1,PAT-1,PRB-1,Problem narrative one\n" +
            "TP-999,PAT-9,PRB-9,Orphan problem narrative\n");
    }

    public static void WriteDuplicateInputFiles(string inputFolder)
    {
        WriteValidInputFiles(inputFolder);

        WriteFile(inputFolder, "problems.csv",
            "Id,Patient Id,Problem Id,Plan Field Problem Narrative\n" +
            "TP-1,PAT-1,PRB-1,Problem narrative one\n" +
            "TP-1,PAT-1,PRB-1,Duplicate problem narrative\n");
    }

    public static void WriteMissingRequiredInputFiles(string inputFolder)
    {
        WriteValidInputFiles(inputFolder);

        WriteFile(inputFolder, "treatment_plans.csv",
            "Id,Patient Id\n" +
            ",\n");
    }

    public static void WriteWarningsOnlyInputFiles(string inputFolder)
    {
        WriteValidInputFiles(inputFolder);

        WriteFile(inputFolder, "problems.csv",
            "Id,Patient Id,Problem Id,Plan Field Problem Narrative\n" +
            "TP-1,PAT-1,PRB-1,\n" +
            "TP-2,PAT-2,PRB-2,Problem narrative two\n");
    }
}