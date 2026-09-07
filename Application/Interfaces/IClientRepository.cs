using Domain.Entities;

namespace Application.Interfaces;

public interface IClientRepository
{
    Task<List<Client>> GetAllAsync();
    Task BulkInsertAsync(IEnumerable<Client> clients);
}