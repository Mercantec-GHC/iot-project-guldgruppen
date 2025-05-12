using backend.Models;

namespace backend.Services;

public interface IMailService
{
    // Metode til at sende en mail- accepterer Maildata som input og returnerer en boolean for at vise success eller fejl.
    bool SendMail(MailData Mail_Data);
}