using backend.Models;

namespace backend.Repositories;

public interface ISensorRepository
{
    Task<IEnumerable<SensorReading>> GetAllAsync();
    Task<SensorReading> GetByIdAsync(int id);
    Task AddAsync(SensorReading reading);
    Task UpsertAsync(SensorReading reading);
    Task DeleteAllAsync();

    // New method to fetch readings by ArduinoId
    Task<IEnumerable<SensorReading>> GetByArduinoIdAsync(string arduinoId);
    Task DeleteIfExistsAsync(string dtoArduinoId);
}