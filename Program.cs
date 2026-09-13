using Microsoft.Extensions.Configuration;
using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

Console.WriteLine("Treatment Plan Combiner");
Console.WriteLine();

try
{
    var settings = LoadSettings();

    return (int)new ApplicationRunner(settings).Run();
}
catch (Exception exception)
{
    var errorLogger = new ApplicationErrorLogger();
    errorLogger.Write("Logs", exception);

    Console.WriteLine();
    Console.WriteLine("Fatal application error.");
    Console.WriteLine(exception.Message);
    Console.WriteLine("An error log was created in the Logs folder.");

    return (int)ApplicationExitCode.FatalError;
}

static AppSettings LoadSettings()
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .Build();

    var settings = configuration.Get<AppSettings>();

    if (settings is null)
    {
        throw new InvalidOperationException("Could not load appsettings.json.");
    }

    return settings;
}