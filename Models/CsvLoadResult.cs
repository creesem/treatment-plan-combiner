namespace TreatmentPlanCombiner.Models;

public class CsvLoadIssue
{
    public string SourceFile { get; set; } = "";
    public int Row { get; set; }
    public string Message { get; set; } = "";
}

public class CsvLoadResult<T>
{
    public List<T> Records { get; set; } = new();
    public List<CsvLoadIssue> Issues { get; set; } = new();
}