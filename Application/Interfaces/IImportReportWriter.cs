using Application.Dtos;

namespace Application.Interfaces;

public interface IImportReportWriter
{
    Task WriteAsync(IEnumerable<WorkOrderImportResult> results, string outputPath);
}