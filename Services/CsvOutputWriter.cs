using System.Globalization;
using CsvHelper;

namespace TreatmentPlanCombiner.Services;

public class CsvOutputWriter
{
    public void Write<T>(string filePath, List<T> records)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(records);
    }
}