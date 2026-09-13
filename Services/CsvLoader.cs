using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class CsvLoader
{
    public CsvLoadResult<T> Load<T>(string filePath)
    {
        var records = new List<T>();
        var issues = new List<CsvLoadIssue>();

        using var reader = new StreamReader(
            filePath,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        using var csv = new CsvReader(
            reader,
            CreateConfiguration(issues, filePath));

        if (!csv.Read())
        {
            issues.Add(new CsvLoadIssue
            {
                SourceFile = filePath,
                Row = 0,
                Message = "File is empty; no header record was found."
            });

            return new CsvLoadResult<T>
            {
                Records = records,
                Issues = issues
            };
        }

        csv.ReadHeader();

        while (csv.Read())
        {
            try
            {
                var record = csv.GetRecord<T>();

                if (record is not null)
                {
                    records.Add(record);
                }
            }
            catch (Exception exception)
            {
                issues.Add(new CsvLoadIssue
                {
                    SourceFile = filePath,
                    Row = csv.Parser.Row,
                    Message = exception.Message
                });
            }
        }

        return new CsvLoadResult<T>
        {
            Records = records,
            Issues = issues
        };
    }

    private static CsvConfiguration CreateConfiguration(
        List<CsvLoadIssue> issues,
        string filePath)
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => NormalizeHeader(args.Header),
            BadDataFound = context =>
            {
                issues.Add(new CsvLoadIssue
                {
                    SourceFile = filePath,
                    Row = context.Context?.Parser?.Row ?? 0,
                    Message = $"Row could not be parsed: '{TrimRawRecord(context.RawRecord ?? string.Empty)}'."
                });
            }
        };
    }

    private static string NormalizeHeader(string header)
    {
        return header
            .Trim()
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }

    private static string TrimRawRecord(string rawRecord)
    {
        const int maxLength = 120;

        var value = rawRecord.Replace("\r", string.Empty).Replace("\n", string.Empty);

        return value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength) + "...";
    }
}