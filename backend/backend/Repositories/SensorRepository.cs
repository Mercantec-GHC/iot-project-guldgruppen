using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

// Implementering af ISensorRepository til håndtering af sensoraflæsninger i databasen
public class SensorRepository : ISensorRepository
{
    private readonly AppDbContext _context; // Databasekontekst til dataadgang

    // Constructor med dependency injection af databasekontekst
    public SensorRepository(AppDbContext context)
    {
        _context = context;
    }

    // Henter alle sensoraflæsninger fra databasen asynkront
    public async Task<IEnumerable<SensorReading>> GetAllAsync()
    {
        return await _context.SensorReadings.ToListAsync();
    }

    // Finder en specifik sensoraflæsning baseret på ID asynkront
    public async Task<SensorReading> GetByIdAsync(int id)
    {
        return await _context.SensorReadings.FindAsync(id);
    }

    // Henter alle aflæsninger for en specifik Arduino-enhed asynkront
    public async Task<IEnumerable<SensorReading>> GetByArduinoIdAsync(string arduinoId)
    {
        return await _context.SensorReadings
            .Where(r => r.ArduinoId == arduinoId) // Filtrer på ArduinoId
            .ToListAsync();
    }

    // Tilføjer en ny sensoraflæsning til databasen asynkront
    public async Task AddAsync(SensorReading reading)
    {
        await _context.SensorReadings.AddAsync(reading);
        await _context.SaveChangesAsync(); // Gemmer ændringer
    }
    
    // Opdaterer eksisterende aflæsning eller tilføjer ny hvis den ikke findes (UPSERT)
    public async Task UpsertAsync(SensorReading reading)
    {
        // Søger efter eksisterende aflæsning for samme Arduino-enhed
        var existing = await _context.SensorReadings
            .FirstOrDefaultAsync(r => r.ArduinoId == reading.ArduinoId);
    
        if (existing != null) // Hvis aflæsning findes: opdater
        {
            existing.Temperature = reading.Temperature;
            existing.MotionDetected = reading.MotionDetected;
            existing.HumidityLevel = reading.HumidityLevel;
            existing.Timestamp = DateTime.UtcNow; // Opdaterer tidsstempel

            _context.SensorReadings.Update(existing);
        }
        else // Hvis aflæsning ikke findes: tilføj ny
        {
            await _context.SensorReadings.AddAsync(reading);
        }

        await _context.SaveChangesAsync(); // Gemmer ændringer
    }

    // Sletter alle sensoraflæsninger i databasen asynkront
    public async Task DeleteAllAsync()
    {
        _context.SensorReadings.RemoveRange(_context.SensorReadings);
        await _context.SaveChangesAsync();
    }

    // Sletter aflæsninger for en specifik Arduino-enhed hvis de findes
    public async Task DeleteIfExistsAsync(string arduinoId)
    {
        // Finder aflæsning for den angivne Arduino-enhed
        var existingReading = await _context.SensorReadings
            .FirstOrDefaultAsync(r => r.ArduinoId == arduinoId);

        if (existingReading != null) // Hvis aflæsning findes
        {
            _context.SensorReadings.Remove(existingReading); // Slet aflæsning
            await _context.SaveChangesAsync(); // Gemmer ændringer
        }
    }
}