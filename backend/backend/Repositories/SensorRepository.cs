using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly AppDbContext _context;

    public SensorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SensorReading>> GetAllAsync()
    {
        return await _context.SensorReadings.ToListAsync();
    }

    public async Task<SensorReading> GetByIdAsync(int id)
    {
        return await _context.SensorReadings.FindAsync(id);
    }

    public async Task AddAsync(SensorReading reading)
    {
        await _context.SensorReadings.AddAsync(reading);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAllAsync()
    {
        _context.SensorReadings.RemoveRange(_context.SensorReadings);
        await _context.SaveChangesAsync();
    }
}