using Application.Interfaces;
using Domain.Entities;

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
}