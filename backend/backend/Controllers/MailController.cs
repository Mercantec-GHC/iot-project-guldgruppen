using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class MailController : ControllerBase
{
    // Referance til mail servicen - bruges til at sende mails.
    private readonly IMailService Mail_Service;
    private readonly AppDbContext _context;

    // Constructor til at injekte IMailService og AppDbContext dependencies.
    public MailController(IMailService _MailService, AppDbContext context)
    {
        Mail_Service = _MailService;  // Tildel den injektede mail service til class-level variablen.
        _context = context;           // Tildel den injektede database context.
    }

    // HTTP POST metode til at sende en mail.
    [HttpPost]
    public bool SendMail(MailData Mail_Data)
    {
        // Kalder SendMail metoden fra den injektede mail service for at sende den skrevne mail data.
        return Mail_Service.SendMail(Mail_Data);
    }

    // HTTP POST metode til at sende sensor data til brugerens email fra Arduino.
    [HttpPost("send-sensor-reading")]
    public async Task<IActionResult> SendSensorReading([FromBody] string arduinoId)
    {
        // Find brugeren associeret med ArduinoId.
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ArduinoId == arduinoId);
        if (user == null)
        {
            return NotFound("User with the specified ArduinoId not found.");
        }

        // Find den seneste sensoraflæsning for ArduinoId.
        var sensorReading = await _context.SensorReadings
            .Where(sr => sr.ArduinoId == arduinoId)
            .OrderByDescending(sr => sr.Timestamp)
            .FirstOrDefaultAsync();

        if (sensorReading == null)
        {
            return NotFound("No sensor readings found for the specified ArduinoId.");
        }

        // Forbered email data.
        var mailData = new MailData
        {
            EmailToId = user.Email,
            EmailToName = user.Username,
            EmailSubject = "Latest Sensor Reading",
            EmailBody = $"Hello {user.Username},\n\n" +
                        $"Here is the latest sensor reading for your Arduino device:\n" +
                        $"- Temperature: {sensorReading.Temperature}\n" +
                        $"- Motion Detected: {sensorReading.MotionDetected}\n" +
                        $"- Humidity Level: {sensorReading.HumidityLevel}\n" +
                        $"- Timestamp: {sensorReading.Timestamp}\n\n" +
                        "Best regards,\nClimate Control Center"
        };

        // Send email via mail service.
        var emailSent = Mail_Service.SendMail(mailData);
        if (!emailSent)
        {
            return StatusCode(500, "Failed to send the email.");
        }

        return Ok("Sensor reading email sent successfully.");
    }
}