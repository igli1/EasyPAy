using System.Globalization;
using System.Text;
using Application.Dtos;

namespace Application.Helpers;

public class CsvBuilder
{
    public static byte[] BuildWorkOrderCsv(IEnumerable<WorkOrderReportRow> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("WorkOrderId,TechnicianName,ClientName,Information,Date,Total");

        foreach (var r in rows)
        {
            builder.AppendLine(string.Join(",",
                r.WorkOrderId,
                Escape(r.TechnicianName),
                Escape(r.ClientName),
                Escape(r.Information),
                r.Date.ToString("yyyy-MM-dd"),
                r.Total.ToString(CultureInfo.InvariantCulture)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}