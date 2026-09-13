namespace TreatmentPlanCombiner.Models;

public class ProblemRecord
{
    public string Id { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string ProblemId { get; set; } = "";
    public string PlanFieldProblemCurrentDiagnosis { get; set; } = "";
    public string PlanFieldProblemNarrative { get; set; } = "";
    public string PlanFieldProblemPriority { get; set; } = "";
    public string PlanFieldProblemStatus { get; set; } = "";
    public string PlanFieldProblemType { get; set; } = "";
}