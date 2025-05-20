namespace backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

[ApiController]
[Route("api/[controller]")] // Basisrute: /api/sms
public class SmsController : ControllerBase
{
    private readonly IConfiguration _config; // Tilgang til app-indstillinger

    // Constructor med dependency injection af konfiguration
    public SmsController(IConfiguration config)
    {
        _config = config;
    }

    // POST: api/sms/send
    [HttpPost("send")]
    public IActionResult SendSms([FromBody] SmsRequest request)
    {
        // Hent Twilio legitimationsoplysninger fra konfiguration
        var accountSid = _config["Twilio:AccountSid"]; // Twilio konto-SID
        var authToken = _config["Twilio:AuthToken"]; // Twilio auth token
        var fromNumber = _config["Twilio:FromNumber"]; // Afsendernummer

        // Initialiser Twilio klienten med legitimationsoplysninger
        TwilioClient.Init(accountSid, authToken);

        // Send SMS besked via Twilio API
        var message = MessageResource.Create(
            to: new PhoneNumber(request.To), // Modtagertelefonnummer
            from: new PhoneNumber(fromNumber), // Afsendernummer
            body: request.Message // Beskedtekst
        );
        
        // Returner Twilio's message SID som bekræftelse
        return Ok(new { messageSid = message.Sid });
    }
}

// Request model til SMS sending
public class SmsRequest
{
    public string To { get; set; } // Modtagerens telefonnummer
    public string Message { get; set; } // Beskedens indhold
}