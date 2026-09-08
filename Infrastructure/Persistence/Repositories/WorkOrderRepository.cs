using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly EasyPayDbContext _context;
    public WorkOrderRepository(EasyPayDbContext context)
    {
        _context = context;
    }

    public async Task BulkInsertAsync(IEnumerable<WorkOrder> workOrders)
    {
        await _context.WorkOrders.AddRangeAsync(workOrders.ToList());
        await _context.SaveChangesAsync();
    }
    public async Task<List<WorkOrderReportRow>> GetAllForReportAsync()
    {
        return await _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Client)
            .Include(w => w.Technician)
            .Select(w => new WorkOrderReportRow
            {
                WorkOrderId = w.Id,
                TechnicianName = $"{w.Technician.FirstName} {w.Technician.LastName}",
                ClientName = $"{w.Client.FirstName} {w.Client.LastName}",
                Information = w.Information,
                Date = w.Date,
                Total = w.Total
            })
            .ToListAsync();
    }
}