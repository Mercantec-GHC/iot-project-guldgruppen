using backend.Models;

namespace backend.Repositories;

public interface ISensorRepository
{
    Task<IEnumerable<SensorReading>> GetAllAsync();
    Task<SensorReading> GetByIdAsync(int id);
    Task AddAsync(SensorReading reading);
    Task DeleteAllAsync();
}