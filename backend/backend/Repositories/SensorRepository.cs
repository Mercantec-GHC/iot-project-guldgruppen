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
    
    public async Task UpsertAsync(SensorReading reading)
    {
        var existing = await _context.SensorReadings.FirstOrDefaultAsync();
    
        if (existing != null)
        {
            // Update the existing reading
            existing.Temperature = reading.Temperature;
            existing.MotionDetected = reading.MotionDetected;
            existing.MoistureLevel = reading.MoistureLevel;
            existing.Timestamp = DateTime.UtcNow;

            _context.SensorReadings.Update(existing);
        }
        else
        {
            // Insert a new reading
            await _context.SensorReadings.AddAsync(reading);
        }

        await _context.SaveChangesAsync();
    }

    
    public async Task DeleteAllAsync()
    {
        _context.SensorReadings.RemoveRange(_context.SensorReadings);
        await _context.SaveChangesAsync();
    }
}