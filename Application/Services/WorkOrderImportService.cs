using Application.Dtos;
using Application.Extraction;
using Application.Helpers;
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

    private const int BatchSize = 5000;

    public WorkOrderImportService(
        IExcelReader<RawWorkOrderRow> workOrderReader,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IWorkOrderRepository workOrderRepository,
        INameMatcher nameMatcher)
    {
        _workOrderReader = workOrderReader;
        _clientRepository = clientRepository;
        _technicianRepository = technicianRepository;
        _workOrderRepository = workOrderRepository;
        _nameMatcher = nameMatcher;
    }

    public async Task<ServiceResponseDto<string>> RunAsync(Stream workOrderStream)
    {
        if (workOrderStream is null || workOrderStream.Length == 0)
        {
            return ServiceResponseDto<string>.Fail("Work order file is empty or missing.");
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

        if (clients.Count == 0)
        {
            return ServiceResponseDto<string>.Fail("Clients table is empty. Import the Finance client list first.");
        }

        var technicianLookup = technicians.ToDictionary(
            t => Normalize($"{t.FirstName} {t.LastName}"),
            t => t);

        // Pass 1: discover technicians from the work order data itself (no separate source file exists)
        var distinctTechnicianNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in _workOrderReader.ReadRows(workOrderStream))
        {
            if (!string.IsNullOrWhiteSpace(row.TechnicianName))
                distinctTechnicianNames.Add(row.TechnicianName.Trim());
        }

        var newTechnicianNames = distinctTechnicianNames
            .Where(name => !technicianLookup.ContainsKey(Normalize(name)))
            .ToList();

        if (newTechnicianNames.Count > 0)
        {
            var newTechnicians = newTechnicianNames
                .Select(name => new RawPersonRow { FullName = name })
                .Select(NameConvention.ToTechnician)
                .ToList();

            await _technicianRepository.BulkInsertAsync(newTechnicians);

            technicians = await _technicianRepository.GetAllAsync();
            technicianLookup = technicians.ToDictionary(
                t => Normalize($"{t.FirstName} {t.LastName}"),
                t => t);
        }


        workOrderStream.Seek(0, SeekOrigin.Begin);

        var results = new List<WorkOrderImportResult>();
        var batch = new List<WorkOrder>();
        int rowNumber = 0;

        try
        {
            foreach (var row in _workOrderReader.ReadRows(workOrderStream))
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
        

        int successCount = results.Count(r => r.Success);
        int failCount = results.Count - successCount;

        return ServiceResponseDto<string>.Success(
            $"Import finished. {successCount} succeeded, {failCount} failed.");
    }

    private static string Normalize(string input) =>
        input.Trim().ToLowerInvariant().Replace("ç", "c").Replace("ë", "e");

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
}