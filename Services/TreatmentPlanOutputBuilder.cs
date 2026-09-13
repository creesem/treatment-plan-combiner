using System.Text.Json;
using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class TreatmentPlanOutputBuilder
{
    public List<TreatmentPlanOutputRecord> Build(List<TreatmentPlanHierarchy> hierarchies)
    {
        var outputRecords = new List<TreatmentPlanOutputRecord>();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        foreach (var hierarchy in hierarchies)
        {
            var treatmentPlanData = new TreatmentPlanData
            {
                Problems = hierarchy.Problems
            };

            var json = JsonSerializer.Serialize(treatmentPlanData, jsonOptions);

            outputRecords.Add(new TreatmentPlanOutputRecord
            {
                TreatmentPlanId = hierarchy.TreatmentPlanId,
                PatientId = hierarchy.PatientId,
                TreatmentPlanDataJson = json
            });
        }

        return outputRecords;
    }
}