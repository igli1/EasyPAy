using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Services;
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
    private readonly IWebHostEnvironment _environment;
    
    public ImportController(
        WorkOrderImportService workOrderImportService,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IExcelReader<RawPersonRow> personReader,
        IWebHostEnvironment environment)
    {
        _workOrderImportService = workOrderImportService;
        _clientRepository = clientRepository;
        _technicianRepository = technicianRepository;
        _personReader = personReader;
        _environment = environment;
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
}