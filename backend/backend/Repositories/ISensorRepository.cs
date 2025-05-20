using backend.Models;

namespace backend.Repositories;

// Interface for SensorRepository - definerer alle metoder der skal implementeres
public interface ISensorRepository
{
    // Henter alle sensoraflæsninger asynkront
    Task<IEnumerable<SensorReading>> GetAllAsync();
    
    // Henter en specifik sensoraflæsning baseret på id asynkront
    Task<SensorReading> GetByIdAsync(int id);
    
    // Tilføjer en ny sensoraflæsning asynkront
    Task AddAsync(SensorReading reading);
    
    // Opdaterer en eksisterende sensoraflæsning eller tilføjer en ny hvis den ikke findes (upsert)
    Task UpsertAsync(SensorReading reading);
    
    // Sletter alle sensoraflæsninger asynkront
    Task DeleteAllAsync();
    
    // Henter alle sensoraflæsninger for en specifik Arduino-enhed baseret på dens ID asynkront
    Task<IEnumerable<SensorReading>> GetByArduinoIdAsync(string arduinoId);
    
    // Sletter sensoraflæsninger hvis de findes for en given Arduino-enhed
    Task DeleteIfExistsAsync(string dtoArduinoId);
}