namespace backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly IConfiguration _config;

    public SmsController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("send")]
    public IActionResult SendSms([FromBody] SmsRequest request)
    {
        var accountSid = _config["Twilio:AccountSid"];
        var authToken = _config["Twilio:AuthToken"];
        var fromNumber = _config["Twilio:FromNumber"];

        TwilioClient.Init(accountSid, authToken);

        var message = MessageResource.Create(
            to: new PhoneNumber(request.To),
            from: new PhoneNumber(fromNumber),
            body: request.Message
        );

        return Ok(new { messageSid = message.Sid });
    }
}

public class SmsRequest
{
    public string To { get; set; }
    public string Message { get; set; }
}