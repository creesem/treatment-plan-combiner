namespace TreatmentPlanCombiner.Services;

public class ApplicationErrorLogger
{
    public void Write(string logFolder, Exception exception)
    {
        Directory.CreateDirectory(logFolder);

        var filePath = Path.Combine(
            logFolder,
            $"application_error_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        var contents =
$"""
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Exception Type: {exception.GetType().FullName}

Message:
{exception.Message}

Stack Trace:
{exception.StackTrace}
""";

        File.WriteAllText(filePath, contents);
    }
}