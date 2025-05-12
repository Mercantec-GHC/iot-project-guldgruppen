using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class MailController : ControllerBase
{
    // Referance til mail servicen - bruges til at sende mails.
    IMailService Mail_Service = null;

    // Constructor til at injekte IMailService dependency - aktiverer mail funktionalitet.
    public MailController(IMailService _MailService)
    {
        Mail_Service = _MailService;  // Tildel den injektede mail service til class-level variablen.
    }

    // HTTP POST metode til at sende en mail.
    [HttpPost]
    public bool SendMail(MailData Mail_Data)
    {
        // Kalder SendMail metoden fra den injektede mail service for at sende den skrevne mail data.
        return Mail_Service.SendMail(Mail_Data);
    }
}