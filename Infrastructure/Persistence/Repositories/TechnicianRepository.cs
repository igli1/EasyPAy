using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TechnicianRepository : ITechnicianRepository
    {
    private readonly EasyPayDbContext _context;

    public TechnicianRepository(EasyPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<Technician>> GetAllAsync()
    {
        return await _context.Technicians.AsNoTracking().ToListAsync();
    }

    public async Task BulkInsertAsync(IEnumerable<Technician> technicians)
    {
        await _context.Technicians.AddRangeAsync(technicians);
        await _context.SaveChangesAsync();
    }
}