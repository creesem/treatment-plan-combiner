using System.Text.Json;
using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class DebugJsonWriter
{
    public void Write(
        string outputFolder,
        List<TreatmentPlanHierarchy> hierarchies)
    {
        Directory.CreateDirectory(outputFolder);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        foreach (var hierarchy in hierarchies)
        {
            var fileName = $"{hierarchy.TreatmentPlanId}.json";

            var filePath = Path.Combine(outputFolder, fileName);

            var json = JsonSerializer.Serialize(hierarchy, options);

            File.WriteAllText(filePath, json);
        }
    }
}