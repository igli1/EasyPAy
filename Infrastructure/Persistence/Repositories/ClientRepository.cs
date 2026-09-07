using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ClientRepository: IClientRepository
{
    private readonly EasyPayDbContext _context;

    public ClientRepository(EasyPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<Client>> GetAllAsync()
    {
        return await _context.Clients.AsNoTracking().ToListAsync();
    }

    public async Task BulkInsertAsync(IEnumerable<Client> clients)
    {
        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();
    }
}