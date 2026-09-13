namespace TreatmentPlanCombiner.Services;

public static class IdNormalizer
{
    public static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}