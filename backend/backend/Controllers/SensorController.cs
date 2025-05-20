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
    // Dependency injections
    private readonly ISensorRepository _repository;
    private readonly AppDbContext _context;
    private readonly IMailService _mailService;
    private readonly IConfiguration _config;

    // Constructor med dependency injection
    public SensorController(ISensorRepository repository, AppDbContext context, IMailService mailService, IConfiguration config)
    {
        _repository = repository;
        _context = context;
        _mailService = mailService;
        _config = config;
    }

    // Tjekker om et ArduinoId er gyldig (findes i databasen)
    private async Task<bool> IsValidArduinoId(string arduinoId)
    {
        return await _context.Users.AnyAsync(u => u.ArduinoId == arduinoId);
    }

    // Endpoint for at modtage temperaturdata
    [HttpPost("temperature")]
    public async Task<IActionResult> PostTemperature([FromBody] TemperatureReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        // Opretter et nyt SensorReading objekt
        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            Temperature = dto.Temperature,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Tjekker om temperaturen overstiger en grænseværdi
        await CheckTemperatureThreshold(dto.ArduinoId, dto.Temperature);

        return Ok();
    }

    // Endpoint for at modtage bevægelsesdata
    [HttpPost("motion")]
    public async Task<IActionResult> PostMotion([FromBody] MotionReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        // Finder brugeren der hører til ArduinoId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ArduinoId == dto.ArduinoId);

        if (user == null)
        {
            return BadRequest("No user found for this ArduinoId.");
        }

        // Opretter et nyt SensorReading objekt
        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            MotionDetected = dto.MotionDetected,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Hvis der er bevægelse og brugeren vil have besked
        if (dto.MotionDetected && user.SendEmailAlert)
        {
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastMotionAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5); // 5 minutters cooldown
    
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send notifications...");
                var alertMessage = $"Movement was detected by your sensor (Arduino ID: {dto.ArduinoId}) at {DateTime.UtcNow.ToString("g")}";
        
                // Send email
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Movement Detected!",
                    EmailBody = alertMessage
                };
                var emailSent = _mailService.SendMail(mailData);
        
                // Send SMS hvis telefonnummer findes
                var smsSent = true;
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    smsSent = await SendAlertSms(user, alertMessage);
                }

                if (emailSent || smsSent)
                {
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

    // Endpoint for at modtage fugtighedsdata
    [HttpPost("humidity")]
    public async Task<IActionResult> PostHumidity([FromBody] HumidityReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        // Opretter et nyt SensorReading objekt
        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            HumidityLevel = dto.HumidityLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Tjekker om fugtigheden overstiger en grænseværdi
        await CheckHumidityThreshold(dto.ArduinoId, dto.HumidityLevel);

        return Ok();
    }
    
    // Endpoint for at hente alle målinger for et ArduinoId
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

    // Endpoint for at modtage kombineret data (temperatur, bevægelse, fugtighed)
    [HttpPost("reading")]
    public async Task<IActionResult> PostCombinedReading([FromBody] CombinedReadingDto dto)
    {
        if (!await IsValidArduinoId(dto.ArduinoId))
        {
            return BadRequest("Invalid ArduinoId.");
        }

        // Finder brugeren der hører til ArduinoId
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ArduinoId == dto.ArduinoId);

        if (user == null)
        {
            return BadRequest("No user found for this ArduinoId.");
        }

        // Opretter et nyt SensorReading objekt med alle data
        var reading = new SensorReading
        {
            ArduinoId = dto.ArduinoId,
            Temperature = dto.Temperature,
            MotionDetected = dto.MotionDetected,
            HumidityLevel = dto.HumidityLevel,
            Timestamp = DateTime.UtcNow
        };
        await _repository.UpsertAsync(reading);

        // Tjekker alle grænseværdier
        if (dto.Temperature.HasValue)
        {
            await CheckTemperatureThreshold(dto.ArduinoId, dto.Temperature.Value);
        }

        if (dto.HumidityLevel.HasValue)
        {
            await CheckHumidityThreshold(dto.ArduinoId, dto.HumidityLevel.Value);
        }

        // Hvis der er bevægelse og brugeren vil have besked
        if (dto.MotionDetected && user.SendEmailAlert)
        {
            var timeSinceLastAlert = DateTime.UtcNow - (user.LastMotionAlertSentAt ?? DateTime.MinValue);
            var alertCooldown = TimeSpan.FromMinutes(5); // 5 minutters cooldown
            
            if (timeSinceLastAlert >= alertCooldown)
            {
                Console.WriteLine("Attempting to send motion notifications from combined reading...");
                var alertMessage = $"Movement was detected by your sensor (Arduino ID: {dto.ArduinoId}) at {DateTime.UtcNow.ToString("g")}";
                
                // Send email
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Movement Detected!",
                    EmailBody = alertMessage
                };
                var emailSent = _mailService.SendMail(mailData);
                
                // Send SMS hvis telefonnummer findes
                var smsSent = true;
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    smsSent = await SendAlertSms(user, alertMessage);
                }

                if (emailSent || smsSent)
                {
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

    // Tjekker om temperatur grænseværdi er overskredet og sender besked hvis nødvendigt
    private async Task CheckTemperatureThreshold(string arduinoId, float temperature)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ArduinoId == arduinoId);
        if (user == null || !user.SendTemperatureAlert || !user.TemperatureThreshold.HasValue)
            return;

        if (temperature >= user.TemperatureThreshold.Value)
        {
            var currentTime = DateTime.UtcNow;
            var emailCooldown = TimeSpan.FromMinutes(5);
            var smsCooldown = TimeSpan.FromMinutes(5);

            var timeSinceLastEmail = currentTime - (user.LastTemperatureEmailSentAt ?? DateTime.MinValue);
            var timeSinceLastSms = currentTime - (user.LastTemperatureSmsSentAt ?? DateTime.MinValue);

            var alertMessage = $"Temperature threshold ({user.TemperatureThreshold}°C) was reached by your sensor (Arduino ID: {arduinoId}) at {currentTime:g}. Current temperature: {temperature}°C";

            bool emailSent = false, smsSent = false;

            if (timeSinceLastEmail >= emailCooldown)
            {
                Console.WriteLine("Sending temperature email...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Temperature Threshold Reached!",
                    EmailBody = alertMessage
                };
                emailSent = _mailService.SendMail(mailData);
            }
            else
            {
                Console.WriteLine($"Email not sent - cooldown active. Time remaining: {emailCooldown - timeSinceLastEmail}");
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber) && timeSinceLastSms >= smsCooldown)
            {
                Console.WriteLine("Sending temperature SMS...");
                smsSent = await SendAlertSms(user, alertMessage);
            }
            else
            {
                Console.WriteLine($"SMS not sent - cooldown active. Time remaining: {smsCooldown - timeSinceLastSms}");
            }

            if (emailSent)
                user.LastTemperatureEmailSentAt = currentTime;
            if (smsSent)
                user.LastTemperatureSmsSentAt = currentTime;

            await _context.SaveChangesAsync();
        }
    }

    // Tjekker om fugtigheds grænseværdi er overskredet og sender besked hvis nødvendigt
    private async Task CheckHumidityThreshold(string arduinoId, float humidityLevel)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ArduinoId == arduinoId);
        if (user == null || !user.SendHumidityAlert || !user.HumidityThreshold.HasValue)
            return;

        if (humidityLevel >= user.HumidityThreshold.Value)
        {
            var currentTime = DateTime.UtcNow;
            var emailCooldown = TimeSpan.FromMinutes(5);
            var smsCooldown = TimeSpan.FromMinutes(5);

            var timeSinceLastEmail = currentTime - (user.LastHumidityEmailSentAt ?? DateTime.MinValue);
            var timeSinceLastSms = currentTime - (user.LastHumiditySmsSentAt ?? DateTime.MinValue);

            var alertMessage = $"Humidity threshold ({user.HumidityThreshold}%) reached by your sensor (Arduino ID: {arduinoId}) at {currentTime:g}. Current humidity: {humidityLevel}%";

            bool emailSent = false, smsSent = false;

            if (timeSinceLastEmail >= emailCooldown)
            {
                Console.WriteLine("Sending humidity email...");
                var mailData = new MailData
                {
                    EmailToId = user.Email,
                    EmailToName = user.Username,
                    EmailSubject = "Humidity Threshold Reached!",
                    EmailBody = alertMessage
                };
                emailSent = _mailService.SendMail(mailData);
            }
            else
            {
                Console.WriteLine($"Email not sent - cooldown active. Time remaining: {emailCooldown - timeSinceLastEmail}");
            }

            if (!string.IsNullOrEmpty(user.PhoneNumber) && timeSinceLastSms >= smsCooldown)
            {
                Console.WriteLine("Sending humidity SMS...");
                smsSent = await SendAlertSms(user, alertMessage);
            }
            else
            {
                Console.WriteLine($"SMS not sent - cooldown active. Time remaining: {smsCooldown - timeSinceLastSms}");
            }

            if (emailSent)
                user.LastHumidityEmailSentAt = currentTime;
            if (smsSent)
                user.LastHumiditySmsSentAt = currentTime;

            await _context.SaveChangesAsync();
        }
    }
    
    // Hjælpemetode til at sende SMS beskeder
    private async Task<bool> SendAlertSms(User user, string message)
    {
        try
        {
            var smsRequest = new SmsRequest
            {
                To = user.PhoneNumber,
                Message = message
            };

            var smsController = new SmsController(_config);
            var result = smsController.SendSms(smsRequest);
            return true;
        }
        catch
        {
            return false;
        }
    }
}