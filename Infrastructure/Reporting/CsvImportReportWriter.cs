using System.Globalization;
using System.Text;
using Application.Dtos;
using Application.Interfaces;

namespace Infrastructure.Reporting;

public class CsvImportReportWriter: IImportReportWriter
{
    public async Task WriteAsync(IEnumerable<WorkOrderImportResult> results, string outputPath)
    {
        var builder = new StringBuilder();
        builder.AppendLine("RowNumber,TechnicianName,Success,FailureReason,MatchedClientName,MatchScore,ExtractedDate,RawNotes");

        foreach (var r in results)
        {
            builder.AppendLine(string.Join(",",
                r.RowNumber,
                Escape(r.TechnicianName),
                r.Success,
                Escape(r.FailureReason ?? string.Empty),
                Escape(r.MatchedClientName ?? string.Empty),
                r.MatchScore?.ToString("F3", CultureInfo.InvariantCulture) ?? string.Empty,
                r.ExtractedDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Escape(r.RawNotes)));
        }

        await File.WriteAllTextAsync(outputPath, builder.ToString(), Encoding.UTF8);
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}