using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class ApplicationRunnerTests
{
    [Fact]
    public void ValidInput_Succeeds_AndCreatesOutputAndReport()
    {
        using var temp = new TestDirectory();

        SampleData.WriteValidInputFiles(Path.Combine(temp.Path, "input"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.Success, exitCode);

        var outputFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Output"), "treatment_plans_*.csv"));
        Assert.Equal(3, File.ReadAllLines(outputFile).Length);

        var reportFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "validation_report_*.csv"));
        Assert.Contains("Success", File.ReadAllText(reportFile));
    }

    [Fact]
    public void WarningsOnly_StillCreateOutput()
    {
        using var temp = new TestDirectory();

        SampleData.WriteWarningsOnlyInputFiles(Path.Combine(temp.Path, "input"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.Success, exitCode);

        var outputFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Output"), "treatment_plans_*.csv"));
        Assert.Equal(3, File.ReadAllLines(outputFile).Length);
    }

    [Fact]
    public void ValidationErrors_BlockOutput_ButStillCreateReport()
    {
        using var temp = new TestDirectory();

        SampleData.WriteOrphanInputFiles(Path.Combine(temp.Path, "input"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.ValidationErrors, exitCode);

        Assert.Empty(
            Directory.GetFiles(Path.Combine(temp.Path, "Output"), "treatment_plans_*.csv"));

        var reportFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "validation_report_*.csv"));
        Assert.Contains("CompletedWithErrors", File.ReadAllText(reportFile));
        Assert.Contains("OrphanProblem", File.ReadAllText(reportFile));
    }

    [Fact]
    public void DuplicateIds_BlockOutput()
    {
        using var temp = new TestDirectory();

        SampleData.WriteDuplicateInputFiles(Path.Combine(temp.Path, "input"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.ValidationErrors, exitCode);

        var reportFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "validation_report_*.csv"));
        Assert.Contains("DuplicateProblemId", File.ReadAllText(reportFile));
    }

    [Fact]
    public void MissingRequiredFields_BlockOutput()
    {
        using var temp = new TestDirectory();

        SampleData.WriteMissingRequiredInputFiles(Path.Combine(temp.Path, "input"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.ValidationErrors, exitCode);

        var reportFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "validation_report_*.csv"));
        Assert.Contains("MissingTreatmentPlanId", File.ReadAllText(reportFile));
    }

    [Fact]
    public void MissingInputFiles_ReturnsMissingInputFilesExitCode()
    {
        using var temp = new TestDirectory();

        SampleData.WriteValidInputFiles(Path.Combine(temp.Path, "input"));
        File.Delete(Path.Combine(temp.Path, "input", "activities.csv"));

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.MissingInputFiles, exitCode);
    }

    [Fact]
    public void ConfigurationError_ReturnsConfigurationErrorExitCode()
    {
        using var temp = new TestDirectory();

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        settings.Files.TreatmentPlans = string.Empty;

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.ConfigurationError, exitCode);
    }

    [Fact]
    public void FatalError_IsLogged_AndReturnsFatalExitCode()
    {
        using var temp = new TestDirectory();

        var settings = SampleData.Settings(
            Path.Combine(temp.Path, "input"),
            temp.Path);

        var inputAsFile = Path.Combine(temp.Path, "not-a-directory");
        Directory.CreateDirectory(temp.Path);
        File.WriteAllText(inputAsFile, "occupied");
        settings.Folders.Input = inputAsFile;

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.FatalError, exitCode);

        var errorLog = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "application_error_*.log"));
        Assert.Contains("Exception", File.ReadAllText(errorLog));
    }

    [Fact]
    public void MalformedRows_BlockOutput_AndReportRows()
    {
        using var temp = new TestDirectory();

        var input = Path.Combine(temp.Path, "input");
        SampleData.WriteValidInputFiles(input);

        SampleData.WriteFile(
            input,
            "objectives.csv",
            "Id,Patient Id,Objective Id,Goal Id,Plan Field Objective Narrative\n" +
            "TP-1,PAT-1,OBJ-1,G-1,Objective narrative one\n" +
            "TP-2,PAT-2,OBJ-2,G-2,\"Unterminated quote\n");

        var settings = SampleData.Settings(input, temp.Path);

        var exitCode = new ApplicationRunner(settings).Run();

        Assert.Equal(ApplicationExitCode.ValidationErrors, exitCode);

        var reportFile = Assert.Single(
            Directory.GetFiles(Path.Combine(temp.Path, "Logs"), "validation_report_*.csv"));
        Assert.Contains("MalformedCsvRow", File.ReadAllText(reportFile));
    }
}