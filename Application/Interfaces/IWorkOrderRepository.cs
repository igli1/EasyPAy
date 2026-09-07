using Domain.Entities;

namespace Application.Interfaces;

public interface IWorkOrderRepository
{
    Task BulkInsertAsync(IEnumerable<WorkOrder> workOrders);
}