using backend.Models;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ISensorRepository _repository;
    private readonly AppDbContext _context;
    private readonly IMailService _mailService;

    public SensorController(ISensorRepository repository, AppDbContext context, IMailService mailService)
    {
        _repository = repository;
        _context = context;
        _mailService = mailService;
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

        // Check temperature threshold
        await CheckTemperatureThreshold(dto.ArduinoId, dto.Temperature);

        return Ok();
    }

    [HttpPost("motion")]
    public async Task<IActionResult> PostMotion([FromBody] MotionReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ArduinoId == dto.ArduinoId);

        if (user == null)
        {
            return BadRequest("No user found for this ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            MotionDetected = dto.MotionDetected,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        if (dto.MotionDetected && user.SendEmailAlert)
        {
            // Tjek om der er gået nok tid siden sidste alert
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastMotionAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5); // 5 minutters cooldown
        
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send email notification...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Movement Detected!",
                    EmailBody = $"Movement was detected by your sensor (Arduino ID: {dto.ArduinoId}) at {DateTime.UtcNow.ToString("g")}"
                };

                var emailSent = _mailService.SendMail(mailData);
                Console.WriteLine($"Email send result: {emailSent}");

                if (emailSent)
                {
                    // Update den sidste alert tid kun hvis email blev sendt
                    user.LastMotionAlertSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Console.WriteLine($"Alert not sent - cooldown active. Time remaining: {alertCooldown - timeSinceLastAlert}");
            }
        }

        return Ok();
    }

    [HttpPost("Humidity")]
    public async Task<IActionResult> PostHumidity([FromBody] HumidityReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            HumidityLevel = dto.HumidityLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Check Humidity threshold
        await CheckHumidityThreshold(dto.ArduinoId, dto.HumidityLevel);

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

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ArduinoId == dto.ArduinoId);

        if (user == null)
        {
            return BadRequest("No user found for this ArduinoId.");
        }

        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            Temperature = dto.Temperature,
            MotionDetected = dto.MotionDetected,
            HumidityLevel = dto.HumidityLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Check all thresholds
        if (dto.Temperature.HasValue)
        {
            await CheckTemperatureThreshold(dto.ArduinoId, dto.Temperature.Value);
        }
    
        if (dto.HumidityLevel.HasValue)
        {
            await CheckHumidityThreshold(dto.ArduinoId, dto.HumidityLevel.Value);
        }

        if (dto.MotionDetected && user.SendEmailAlert)
        {
            // Tjek om der er gået nok tid siden sidste alert
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastMotionAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5); // 5 minutters cooldown
            
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send email notification from combined reading...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Movement Detected!",
                    EmailBody = $"Movement was detected by your sensor (Arduino ID: {dto.ArduinoId}) at {DateTime.UtcNow.ToString("g")}"
                };

                var emailSent = _mailService.SendMail(mailData);
                Console.WriteLine($"Email send result: {emailSent}");

                if (emailSent)
                {
                    // Update den sidste alert tid kun hvis email blev sendt
                    user.LastMotionAlertSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Console.WriteLine($"Alert not sent - cooldown active. Time remaining: {alertCooldown - timeSinceLastAlert}");
            }
        }

        return Ok();
    }
    
        private async Task CheckTemperatureThreshold(string arduinoId, float temperature)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ArduinoId == arduinoId);
        if (user == null || !user.SendTemperatureAlert || !user.TemperatureThreshold.HasValue)
            return;

        if (temperature >= user.TemperatureThreshold.Value)
        {
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastTemperatureAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5);
            
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send temperature alert email...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Temperature Threshold Reached!",
                    EmailBody = $"Temperature threshold ({user.TemperatureThreshold}°C) was reached by your sensor (Arduino ID: {arduinoId}) at {DateTime.UtcNow.ToString("g")}. Current temperature: {temperature}°C"
                };

                var emailSent = _mailService.SendMail(mailData);
                Console.WriteLine($"Email send result: {emailSent}");

                if (emailSent)
                {
                    user.LastTemperatureAlertSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    private async Task CheckHumidityThreshold(string arduinoId, int HumidityLevel)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ArduinoId == arduinoId);
        if (user == null || !user.SendHumidityAlert || !user.HumidityThreshold.HasValue)
            return;

        if (HumidityLevel >= user.HumidityThreshold.Value)
        {
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastHumidityAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5);
            
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send Humidity alert email...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Humidity Threshold Reached!",
                    EmailBody = $"Humidity threshold ({user.HumidityThreshold}%) was reached by your sensor (Arduino ID: {arduinoId}) at {DateTime.UtcNow.ToString("g")}. Current Humidity level: {HumidityLevel}%"
                };

                var emailSent = _mailService.SendMail(mailData);
                Console.WriteLine($"Email send result: {emailSent}");

                if (emailSent)
                {
                    user.LastHumidityAlertSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}