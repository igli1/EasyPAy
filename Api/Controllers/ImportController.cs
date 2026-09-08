using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly WorkOrderImportService _workOrderImportService;
    private readonly IClientRepository _clientRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IExcelReader<RawPersonRow> _personReader;
    private readonly IWorkOrderRepository _workOrderRepository;
    
    public ImportController(
        WorkOrderImportService workOrderImportService,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IExcelReader<RawPersonRow> personReader,
        IWorkOrderRepository workOrderRepository)
    {
        _workOrderImportService = workOrderImportService;
        _clientRepository = clientRepository;
        _technicianRepository = technicianRepository;
        _personReader = personReader;
        _workOrderRepository = workOrderRepository;
    }
    
    [HttpPost("clients")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ServiceResponseDto<int>>> ImportClients(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(ServiceResponseDto<int>.Fail("No file uploaded."));

        using var stream = file.OpenReadStream();
        var clients = _personReader.ReadRows(stream).Select(NameConvention.ToClient).ToList();

        await _clientRepository.BulkInsertAsync(clients);

        return Ok(ServiceResponseDto<int>.Success(clients.Count, $"Imported {clients.Count} clients."));
    }
    
    [HttpPost("work-orders")]
    [RequestSizeLimit(500_000_000)]
    public async Task<ActionResult<ServiceResponseDto<string>>> ImportWorkOrders(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(ServiceResponseDto<string>.Fail("No file uploaded."));

        using var stream = file.OpenReadStream();
        var response = await _workOrderImportService.RunAsync(stream);

        return response.Status ? Ok(response) : BadRequest(response);
    }
    
    [HttpGet("work-orders/export")]
    public async Task<IActionResult> ExportWorkOrders()
    {
        var rows = await _workOrderRepository.GetAllForReportAsync();

        if (rows.Count == 0)
            return NotFound(ServiceResponseDto<byte[]>.Fail("No work orders found in the database."));

        var csv = CsvBuilder.BuildWorkOrderCsv(rows);
        var fileName = $"work-orders-export-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(csv, "text/csv", fileName);
    }
}