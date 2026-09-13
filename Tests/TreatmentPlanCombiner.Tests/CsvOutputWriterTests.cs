using System.Globalization;
using CsvHelper;
using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class CsvOutputWriterTests
{
    private readonly CsvOutputWriter _writer = new();

    [Fact]
    public void Write_CreatesOutputDirectoryWhenMissing()
    {
        using var temp = new TestDirectory();

        var filePath = Path.Combine(temp.Path, "nested", "sub", "output.csv");

        _writer.Write(filePath, new List<TreatmentPlanOutputRecord>
        {
            new() { TreatmentPlanId = "TP-1" }
        });

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Write_WritesHeadersAndRecords()
    {
        using var temp = new TestDirectory();

        var filePath = Path.Combine(temp.Path, "output.csv");

        _writer.Write(filePath, new List<TreatmentPlanOutputRecord>
        {
            new()
            {
                TreatmentPlanId = "TP-1",
                PatientId = "PAT-1",
                TreatmentPlanDataJson = "{\"Problems\":[]}"
            }
        });

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        Assert.True(csv.Read());
        Assert.True(csv.ReadHeader());
        Assert.Equal(
            new[] { "TreatmentPlanId", "PatientId", "TreatmentPlanDataJson" },
            csv.HeaderRecord);

        Assert.True(csv.Read());
        Assert.Equal("TP-1", csv.GetField("TreatmentPlanId"));
        Assert.Equal("PAT-1", csv.GetField("PatientId"));
        Assert.Equal("{\"Problems\":[]}", csv.GetField("TreatmentPlanDataJson"));
    }

    [Fact]
    public void Write_EmptyRecords_WritesHeaderOnly()
    {
        using var temp = new TestDirectory();

        var filePath = Path.Combine(temp.Path, "output.csv");

        _writer.Write(filePath, new List<TreatmentPlanOutputRecord>());

        var lines = File.ReadAllLines(filePath);

        Assert.Single(lines);
        Assert.Contains("TreatmentPlanId", lines[0]);
    }
}