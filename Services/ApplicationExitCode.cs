namespace TreatmentPlanCombiner.Services;

public enum ApplicationExitCode
{
    Success = 0,
    ValidationErrors = 1,
    FatalError = 2,
    MissingInputFiles = 3,
    ConfigurationError = 4
}