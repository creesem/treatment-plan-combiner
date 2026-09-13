using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class CsvLoaderTests
{
    private readonly CsvLoader _loader = new();

    [Fact]
    public void LoadsValidRecords_WithHumanReadableHeaders()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(
            temp.Path,
            "plans.csv",
            "Id,Document Type,Provider Id,Provider Email,Patient Id\n" +
            "TP-1,Treatment Plan,PRV-1,a@example.com,PAT-1\n" +
            "TP-2,Treatment Plan,PRV-1,a@example.com,PAT-2\n");

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.Empty(result.Issues);
        Assert.Equal(2, result.Records.Count);
        Assert.Equal("TP-1", result.Records[0].Id);
        Assert.Equal("PAT-1", result.Records[0].PatientId);
        Assert.Equal("Treatment Plan", result.Records[0].DocumentType);
        Assert.Equal("a@example.com", result.Records[0].ProviderEmail);
    }

    [Fact]
    public void MatchesHeadersRegardlessOfCaseSpacesAndUnderscores()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(
            temp.Path,
            "plans.csv",
            "id,patient_id,provider_email\n" +
            "TP-1,PAT-1,a@example.com\n");

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.Empty(result.Issues);
        Assert.Single(result.Records);
        Assert.Equal("TP-1", result.Records[0].Id);
        Assert.Equal("PAT-1", result.Records[0].PatientId);
        Assert.Equal("a@example.com", result.Records[0].ProviderEmail);
    }

    [Fact]
    public void BlankOptionalValuesBecomeEmptyStrings()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(
            temp.Path,
            "plans.csv",
            "Id,Patient Id,Diagnosis Code1,Diagnosis Code1 Name\n" +
            "TP-1,PAT-1,,Major Depressive Disorder\n");

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.Empty(result.Issues);
        Assert.Single(result.Records);
        Assert.Equal(string.Empty, result.Records[0].DiagnosisCode1);
        Assert.Equal("Major Depressive Disorder", result.Records[0].DiagnosisCode1Name);
    }

    [Fact]
    public void MalformedRow_ProducesIssueWithRowNumber()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(
            temp.Path,
            "plans.csv",
            "Id,Patient Id,Diagnosis Code1\n" +
            "TP-1,PAT-1,F32.1\n" +
            "TP-2,PAT-2,\"Unterminated quote\n");

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.NotEmpty(result.Issues);
        var issue = Assert.Single(result.Issues);
        Assert.Contains("plans.csv", issue.SourceFile);
        Assert.True(issue.Row >= 0, $"Expected a row number, got {issue.Row}.");
    }

    [Fact]
    public void EmptyFile_ProducesLoadIssue()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(temp.Path, "plans.csv", string.Empty);

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.Empty(result.Records);
        var issue = Assert.Single(result.Issues);
        Assert.Contains("empty", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HeaderOnlyFile_ProducesNoRecordsAndNoIssues()
    {
        using var temp = new TestDirectory();

        SampleData.WriteFile(
            temp.Path,
            "plans.csv",
            "Id,Patient Id\n");

        var result = _loader.Load<TreatmentPlanRecord>(
            Path.Combine(temp.Path, "plans.csv"));

        Assert.Empty(result.Records);
        Assert.Empty(result.Issues);
    }
}