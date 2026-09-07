using Application.Dtos;
using Application.Extraction;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class WorkOrderImportService
{
    private readonly IExcelReader<RawWorkOrderRow> _workOrderReader;
    private readonly IClientRepository _clientRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly INameMatcher _nameMatcher;
    private readonly IImportReportWriter _reportWriter;

    private const int BatchSize = 5000;

    public WorkOrderImportService(
        IExcelReader<RawWorkOrderRow> workOrderReader,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IWorkOrderRepository workOrderRepository,
        INameMatcher nameMatcher,
        IImportReportWriter reportWriter)
    {
        _workOrderReader = workOrderReader;
        _clientRepository = clientRepository;
        _technicianRepository = technicianRepository;
        _workOrderRepository = workOrderRepository;
        _nameMatcher = nameMatcher;
        _reportWriter = reportWriter;
    }

    public async Task<ServiceResponseDto<string>> RunAsync(string workOrderFilePath, string reportOutputPath)
{
    if (!File.Exists(workOrderFilePath))
    {
        return ServiceResponseDto<string>.Fail($"Work order file not found: {workOrderFilePath}");
    }

    List<Client> clients;
    List<Technician> technicians;

    try
    {
        clients = await _clientRepository.GetAllAsync();
        technicians = await _technicianRepository.GetAllAsync();
    }
    catch (Exception ex)
    {
        return ServiceResponseDto<string>.Fail($"Failed to load reference data: {ex.Message}");
    }

    if (clients.Count == 0 || technicians.Count == 0)
    {
        return ServiceResponseDto<string>.Fail("Clients or Technicians table is empty. Import those first.");
    }

    var technicianLookup = technicians.ToDictionary(
        t => Normalize($"{t.FirstName} {t.LastName}"),
        t => t);

    var results = new List<WorkOrderImportResult>();
    var batch = new List<WorkOrder>();
    int rowNumber = 0;

    try
    {
        await using var fileStream = new FileStream(workOrderFilePath, FileMode.Open, FileAccess.Read);

        foreach (var row in _workOrderReader.ReadRows(fileStream))
        {
            rowNumber++;
            var result = ProcessRow(row, rowNumber, clients, technicianLookup, batch);
            results.Add(result);

            if (batch.Count >= BatchSize)
            {
                    await _workOrderRepository.BulkInsertAsync(batch);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await _workOrderRepository.BulkInsertAsync(batch);
            }
        }
        catch (Exception ex)
        {
            return ServiceResponseDto<string>.Fail($"Import failed at row {rowNumber}: {ex.Message}");
        }

        await _reportWriter.WriteAsync(results, reportOutputPath);

        int successCount = results.Count(r => r.Success);
        int failCount = results.Count - successCount;

            return ServiceResponseDto<string>.Success(
            reportOutputPath,
            $"Import finished. {successCount} succeeded, {failCount} failed. Report at {reportOutputPath}");
    }

    private WorkOrderImportResult ProcessRow(
        RawWorkOrderRow row,
        int rowNumber,
        List<Client> clients,
        Dictionary<string, Technician> technicianLookup,
        List<WorkOrder> batch)
    {
        var result = new WorkOrderImportResult
        {
            RowNumber = rowNumber,
            TechnicianName = row.TechnicianName,
            RawNotes = row.Notes
        };

        if (!technicianLookup.TryGetValue(Normalize(row.TechnicianName), out var technician))
        {
            result.Success = false;
            result.FailureReason = "Technician not found";
            return result;
        }

        var date = NotesExtractor.ExtractDate(row.Notes);
        if (date is null)
        {
            result.Success = false;
            result.FailureReason = "Could not extract a valid date from Notes";
            return result;
        }

        result.ExtractedDate = date;

        var match = _nameMatcher.FindBestMatch(row.Notes, clients);
        if (match is null)
        {
            result.Success = false;
            result.FailureReason = "No confident client match found in Notes";
            return result;
        }

        result.MatchedClientName = $"{match.Client.FirstName} {match.Client.LastName}";
        result.MatchScore = match.Score;
        result.Success = true;

        batch.Add(new WorkOrder
        {
            ClientId = match.Client.Id,
            TechnicianId = technician.Id,
            Information = row.Notes,
            Date = date.Value,
            Total = row.Total
        });

        return result;
    }

    private static string Normalize(string input) =>
        input.Trim().ToLowerInvariant().Replace("ç", "c").Replace("ë", "e");
}