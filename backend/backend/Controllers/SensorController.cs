using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ISensorRepository _repository;
    private readonly AppDbContext _context;

    public SensorController(ISensorRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    private async Task<bool> IsValidArduinoId(string arduinoId)
    {
        return await _context.Users.AnyAsync(u => u.ArduinoId == arduinoId);
    }

    [HttpPost("temperature")]
    public async Task<IActionResult> PostTemperature([FromBody] TemperatureReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            Temperature = dto.Temperature,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);
        return Ok();
    }

    [HttpPost("motion")]
    public async Task<IActionResult> PostMotion([FromBody] MotionReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            MotionDetected = dto.MotionDetected,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);
        return Ok();
    }

    [HttpPost("moisture")]
    public async Task<IActionResult> PostMoisture([FromBody] MoistureReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            MoistureLevel = dto.MoistureLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);
        return Ok();
    }
    
    [HttpGet("{arduinoId}")]
    public async Task<IActionResult> GetByArduinoId(string arduinoId)
    {
        var readings = await _repository.GetByArduinoIdAsync(arduinoId);
        if (readings == null || !readings.Any())
        {
            return NotFound($"No readings found for ArduinoId: {arduinoId}");
        }
        return Ok(readings);
    }

    [HttpPost("reading")]
    public async Task<IActionResult> PostCombinedReading([FromBody] CombinedReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        await _repository.DeleteIfExistsAsync(dto.ArduinoId);

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            Temperature = dto.Temperature,
            MotionDetected = dto.MotionDetected,
            MoistureLevel = dto.MoistureLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);
        return Ok();
    }
}