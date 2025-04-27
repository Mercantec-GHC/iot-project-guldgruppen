using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ISensorRepository _repository;

    public SensorController(ISensorRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("temperature")]
    public async Task<IActionResult> PostTemperature([FromBody] TemperatureReadingDto dto)
    {
        var reading = new SensorReading
        {
            Temperature = dto.Temperature,
            Timestamp = DateTime.UtcNow
        };
        await _repository.AddAsync(reading);
        return Ok();
    }

    [HttpPost("motion")]
    public async Task<IActionResult> PostMotion([FromBody] MotionReadingDto dto)
    {
        var reading = new SensorReading
        {
            MotionDetected = dto.MotionDetected,
            Timestamp = DateTime.UtcNow
        };
        await _repository.AddAsync(reading);
        return Ok();
    }

    [HttpPost("moisture")]
    public async Task<IActionResult> PostMoisture([FromBody] MoistureReadingDto dto)
    {
        var reading = new SensorReading
        {
            MoistureLevel = dto.MoistureLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.AddAsync(reading);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var readings = await _repository.GetAllAsync();
        return Ok(readings);
    }
    
    [HttpPost("reading")]
    public async Task<IActionResult> PostCombinedReading([FromBody] CombinedReadingDto dto)
    {
        var reading = new SensorReading
        {
            Temperature = dto.Temperature,
            MotionDetected = dto.MotionDetected,
            MoistureLevel = dto.MoistureLevel,
            Timestamp = DateTime.UtcNow
        };
    
        await _repository.AddAsync(reading);
        return Ok();
    }
}

// DTO classes

public class CombinedReadingDto
{
    public float Temperature { get; set; }
    public bool MotionDetected { get; set; }
    public int MoistureLevel { get; set; }
}
public class TemperatureReadingDto
{
    public float Temperature { get; set; }
}

public class MotionReadingDto
{
    public bool MotionDetected { get; set; }
}

public class MoistureReadingDto
{
    public int MoistureLevel { get; set; }
}