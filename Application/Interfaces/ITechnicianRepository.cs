using Domain.Entities;

namespace Application.Interfaces;

public interface ITechnicianRepository
{
    Task<List<Technician>> GetAllAsync();
    Task BulkInsertAsync(IEnumerable<Technician> technicians);
}