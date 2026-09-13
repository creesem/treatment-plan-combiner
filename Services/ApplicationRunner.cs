using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class ApplicationRunner
{
    private readonly AppSettings _settings;
    private readonly CsvLoader _csvLoader;
    private readonly ValidationService _validationService;
    private readonly HierarchyBuilder _hierarchyBuilder;
    private readonly TreatmentPlanOutputBuilder _outputBuilder;
    private readonly CsvOutputWriter _outputWriter;
    private readonly ValidationReportWriter _reportWriter;
    private readonly DebugJsonWriter _debugJsonWriter;
    private readonly ApplicationErrorLogger _errorLogger;
    private readonly InputFileValidator _inputFileValidator;
    private readonly SettingsValidator _settingsValidator;
    private readonly FolderInitializer _folderInitializer;

    public ApplicationRunner(AppSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        _csvLoader = new CsvLoader();
        _validationService = new ValidationService();
        _hierarchyBuilder = new HierarchyBuilder();
        _outputBuilder = new TreatmentPlanOutputBuilder();
        _outputWriter = new CsvOutputWriter();
        _reportWriter = new ValidationReportWriter();
        _debugJsonWriter = new DebugJsonWriter();
        _errorLogger = new ApplicationErrorLogger();
        _inputFileValidator = new InputFileValidator();
        _settingsValidator = new SettingsValidator();
        _folderInitializer = new FolderInitializer();
    }

    public ApplicationExitCode Run()
    {
        try
        {
            return RunInternal();
        }
        catch (Exception exception)
        {
            _errorLogger.Write(_settings.Folders.Logs, exception);

            Console.WriteLine();
            Console.WriteLine("Fatal application error.");
            Console.WriteLine(exception.Message);
            Console.WriteLine($"An error log was created in the {_settings.Folders.Logs} folder.");

            return ApplicationExitCode.FatalError;
        }
    }

    private ApplicationExitCode RunInternal()
    {
        var configurationErrors = _settingsValidator.Validate(_settings);

        if (configurationErrors.Count > 0)
        {
            PrintConfigurationErrors(configurationErrors);
            return ApplicationExitCode.ConfigurationError;
        }

        _folderInitializer.EnsureFoldersExist(_settings);
        Console.WriteLine("Folders Verified");
        Console.WriteLine();

        var missingFiles = _inputFileValidator.Validate(_settings);

        if (missingFiles.Count > 0)
        {
            PrintMissingFiles(missingFiles);
            return ApplicationExitCode.MissingInputFiles;
        }

        var (inputData, loadIssues) = LoadInputData();

        PrintFileCounts(inputData);

        var validationIssues = _validationService.Validate(
            inputData.TreatmentPlans,
            inputData.Problems,
            inputData.Goals,
            inputData.Objectives,
            inputData.Interventions);

        validationIssues.AddRange(loadIssues);

        PrintValidationIssues(validationIssues);

        var validationReportPath = BuildPath(
            _settings.Folders.Logs,
            _settings.Files.ValidationReportPattern);

        if (_validationService.HasBlockingIssues(validationIssues))
        {
            WriteValidationReport(
                validationReportPath,
                validationIssues,
                inputData,
                outputRecordCount: 0);

            Console.WriteLine();
            Console.WriteLine("Validation errors found. Output file will not be created.");
            Console.WriteLine("Review the validation report before continuing.");
            Console.WriteLine($"Validation report created: {validationReportPath}");

            return ApplicationExitCode.ValidationErrors;
        }

        var hierarchies = _hierarchyBuilder.Build(
            inputData.TreatmentPlans,
            inputData.Problems,
            inputData.Goals,
            inputData.Objectives,
            inputData.Interventions);

        Console.WriteLine($"Hierarchies Built: {hierarchies.Count}");

        _debugJsonWriter.Write(_settings.Folders.DebugJson, hierarchies);
        Console.WriteLine($"Debug JSON files created: {hierarchies.Count}");

        var outputRecords = _outputBuilder.Build(hierarchies);
        Console.WriteLine($"Output Records: {outputRecords.Count}");

        var outputFilePath = BuildPath(
            _settings.Folders.Output,
            _settings.Files.OutputFilePattern);

        _outputWriter.Write(outputFilePath, outputRecords);
        Console.WriteLine($"Output file created: {outputFilePath}");

        WriteValidationReport(
            validationReportPath,
            validationIssues,
            inputData,
            outputRecords.Count);

        PrintRunSummary(inputData, validationIssues, hierarchies.Count, outputRecords.Count);

        PrintFirstOutputRecord(outputRecords);

        return ApplicationExitCode.Success;
    }

    private (InputData Data, List<ValidationIssue> LoadIssues) LoadInputData()
    {
        var inputData = new InputData();
        var loadIssues = new List<ValidationIssue>();

        LoadData(inputData.TreatmentPlans, _settings.Files.TreatmentPlans, loadIssues);
        LoadData(inputData.Problems, _settings.Files.Problems, loadIssues);
        LoadData(inputData.Goals, _settings.Files.Goals, loadIssues);
        LoadData(inputData.Objectives, _settings.Files.Objectives, loadIssues);
        LoadData(inputData.Interventions, _settings.Files.Interventions, loadIssues);
        LoadData(inputData.Activities, _settings.Files.Activities, loadIssues);

        return (inputData, loadIssues);
    }

    private void LoadData<T>(List<T> target, string fileName, List<ValidationIssue> loadIssues)
    {
        var filePath = Path.Combine(_settings.Folders.Input, fileName);

        var result = _csvLoader.Load<T>(filePath);

        target.AddRange(result.Records);

        foreach (var issue in result.Issues)
        {
            loadIssues.Add(new ValidationIssue
            {
                Severity = IssueSeverity.Error,
                IssueType = IssueType.MalformedCsvRow,
                Message = $"{issue.SourceFile} (row {issue.Row}): {issue.Message}"
            });
        }
    }

    private void PrintFileCounts(InputData inputData)
    {
        Console.WriteLine("Files Loaded");
        Console.WriteLine();
        Console.WriteLine($"Treatment Plans: {inputData.TreatmentPlans.Count}");
        Console.WriteLine($"Problems: {inputData.Problems.Count}");
        Console.WriteLine($"Goals: {inputData.Goals.Count}");
        Console.WriteLine($"Objectives: {inputData.Objectives.Count}");
        Console.WriteLine($"Interventions: {inputData.Interventions.Count}");
        Console.WriteLine($"Activities: {inputData.Activities.Count}");
        Console.WriteLine();
    }

    private void PrintValidationIssues(List<ValidationIssue> issues)
    {
        Console.WriteLine($"Validation Issues: {issues.Count}");

        foreach (var issue in issues)
        {
            Console.WriteLine($"{issue.Severity} | {issue.IssueType} | {issue.Message}");
        }

        Console.WriteLine();
    }

    private void PrintRunSummary(
        InputData inputData,
        List<ValidationIssue> issues,
        int hierarchyCount,
        int outputRecordCount)
    {
        var warningCount = issues.Count(x => x.Severity == IssueSeverity.Warning);
        var errorCount = issues.Count(x => x.Severity == IssueSeverity.Error);

        Console.WriteLine();
        Console.WriteLine("Run Summary");
        Console.WriteLine();
        Console.WriteLine($"Treatment Plans Loaded: {inputData.TreatmentPlans.Count}");
        Console.WriteLine($"Health Records Loaded: {inputData.Problems.Count + inputData.Goals.Count + inputData.Objectives.Count + inputData.Interventions.Count}");
        Console.WriteLine($"Activities Loaded: {inputData.Activities.Count}");
        Console.WriteLine($"Warnings: {warningCount}");
        Console.WriteLine($"Errors: {errorCount}");
        Console.WriteLine($"Hierarchies Built: {hierarchyCount}");
        Console.WriteLine($"Output Records: {outputRecordCount}");
    }

    private void PrintFirstOutputRecord(List<TreatmentPlanOutputRecord> outputRecords)
    {
        if (outputRecords.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("First Output Record:");
        Console.WriteLine($"Treatment Plan Id: {outputRecords[0].TreatmentPlanId}");
        Console.WriteLine($"Patient Id: {outputRecords[0].PatientId}");

        var preview = outputRecords[0].TreatmentPlanDataJson;

        if (preview.Length > 500)
        {
            preview = preview.Substring(0, 500) + "...";
        }

        Console.WriteLine(preview);
    }

    private void WriteValidationReport(
        string filePath,
        List<ValidationIssue> issues,
        InputData inputData,
        int outputRecordCount)
    {
        _reportWriter.Write(
            filePath,
            issues,
            inputData.TreatmentPlans.Count,
            inputData.Problems.Count,
            inputData.Goals.Count,
            inputData.Objectives.Count,
            inputData.Interventions.Count,
            inputData.Activities.Count,
            outputRecordCount);

        Console.WriteLine($"Validation report created: {filePath}");
    }

    private static void PrintConfigurationErrors(List<string> errors)
    {
        Console.WriteLine("Configuration errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        Console.WriteLine();
        Console.WriteLine("Fix the settings in appsettings.json and run the application again.");
    }

    private static void PrintMissingFiles(List<string> missingFiles)
    {
        Console.WriteLine("Missing required input files:");
        foreach (var missingFile in missingFiles)
        {
            Console.WriteLine($"- {missingFile}");
        }
        Console.WriteLine();
        Console.WriteLine("Please place all required CSV files in the Input folder and run the application again.");
    }

    private static string BuildPath(string folder, string pattern)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var fileName = pattern.Replace("{timestamp}", timestamp);

        return Path.Combine(folder, fileName);
    }
}