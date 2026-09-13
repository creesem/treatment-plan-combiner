namespace TreatmentPlanCombiner.Models;

public class TreatmentPlanRecord
{
    public string Id { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public string ProviderId { get; set; } = "";
    public string ProviderEmail { get; set; } = "";
    public string ProviderFirstName { get; set; } = "";
    public string ProviderLastName { get; set; } = "";
    public string ProviderSupervisor { get; set; } = "";
    public string ProviderProgram { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string DocumentEffectiveDate { get; set; } = "";
    public string DocumentActiveDate { get; set; } = "";
    public string DocumentExpirationDate { get; set; } = "";
    public string CompletedDateTime { get; set; } = "";
    public string UpdatedDateTime { get; set; } = "";
    public string ServiceStartTime { get; set; } = "";
    public string ServiceEndTime { get; set; } = "";
    public string DiagnosisCode1 { get; set; } = "";
    public string DiagnosisCode1Name { get; set; } = "";
    public string DiagnosisCode2 { get; set; } = "";
    public string DiagnosisCode2Name { get; set; } = "";
    public string DiagnosisCode3 { get; set; } = "";
    public string DiagnosisCode3Name { get; set; } = "";
    public string DiagnosisCode4 { get; set; } = "";
    public string DiagnosisCode4Name { get; set; } = "";
    public string DiagnosisCode5 { get; set; } = "";
    public string DiagnosisCode5Name { get; set; } = "";
    public string PlanFieldCustomTextArea1 { get; set; } = "";
    public string PlanFieldCustomTextArea2 { get; set; } = "";
    public string PlanFieldCustomTextbox { get; set; } = "";
}