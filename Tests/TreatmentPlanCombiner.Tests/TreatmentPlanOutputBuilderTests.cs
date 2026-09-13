using System.Text.Json;
using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class TreatmentPlanOutputBuilderTests
{
    private readonly TreatmentPlanOutputBuilder _builder = new();

    [Fact]
    public void Build_CreatesOneOutputRecordPerHierarchy()
    {
        var hierarchies = new List<TreatmentPlanHierarchy>
        {
            CreateHierarchy("TP-1", "PAT-1"),
            CreateHierarchy("TP-2", "PAT-2")
        };

        var records = _builder.Build(hierarchies);

        Assert.Equal(2, records.Count);
    }

    [Fact]
    public void Build_IncludesTreatmentPlanAndPatientIds()
    {
        var hierarchies = new List<TreatmentPlanHierarchy>
        {
            CreateHierarchy("TP-1", "PAT-1")
        };

        var records = _builder.Build(hierarchies);

        var record = Assert.Single(records);
        Assert.Equal("TP-1", record.TreatmentPlanId);
        Assert.Equal("PAT-1", record.PatientId);
    }

    [Fact]
    public void Build_ProducesValidJson()
    {
        var hierarchy = CreateHierarchy("TP-1", "PAT-1");
        hierarchy.Problems.Add(new Problem
        {
            ProblemId = "PRB-1",
            Narrative = "Problem narrative"
        });

        var records = _builder.Build(new List<TreatmentPlanHierarchy> { hierarchy });

        using var document = JsonDocument.Parse(records[0].TreatmentPlanDataJson);

        var root = document.RootElement;
        var problems = root.GetProperty("Problems");

        Assert.Equal(1, problems.GetArrayLength());
        Assert.Equal(
            "PRB-1",
            problems[0].GetProperty("ProblemId").GetString());
    }

    [Fact]
    public void Build_EmptyHierarchies_ReturnsEmptyList()
    {
        var records = _builder.Build(new List<TreatmentPlanHierarchy>());

        Assert.Empty(records);
    }

    [Fact]
    public void Build_EmptyCollections_SerializeConsistently()
    {
        var hierarchy = CreateHierarchy("TP-1", "PAT-1");

        var records = _builder.Build(new List<TreatmentPlanHierarchy> { hierarchy });

        using var document = JsonDocument.Parse(records[0].TreatmentPlanDataJson);

        var problems = document.RootElement.GetProperty("Problems");

        Assert.Equal(0, problems.GetArrayLength());
    }

    private static TreatmentPlanHierarchy CreateHierarchy(string planId, string patientId)
    {
        return new TreatmentPlanHierarchy
        {
            TreatmentPlanId = planId,
            PatientId = patientId
        };
    }
}