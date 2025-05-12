using backend.Configuration;
using backend.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace backend.Services;
    public class MailService : IMailService
    {
        // Mail-indstillinger til konfiguration af SMTP-klienten, såsom vært, port og legitimationsoplysninger.
        MailSettings Mail_Settings = null;

        // Konstruktør til at injicere MailSettings via IOptions, hvilket muliggør konfiguration af mailindstillinger fra en konfigurationsfil.
        public MailService(IOptions<MailSettings> options)
        {
            Mail_Settings = options.Value;   // Tildel de injicerede mailindstillinger til klassens variabel.
        }

        // Metode til at sende en e-mail ved hjælp af MailData-input.
        public bool SendMail(MailData Mail_Data)
        {
            try
            {
                // Opret et nyt MimeMessage-objekt for at opbygge e-mailen.
                MimeMessage email_Message = new MimeMessage();

                // Angiv afsenderens e-mailadresse ved hjælp af indstillingerne.
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
                email_Message.From.Add(email_From);

                // Angiv modtagerens e-mailadresse ud fra MailData-input.
                MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
                email_Message.To.Add(email_To);

                // Angiv emnet for e-mailen.
                email_Message.Subject = Mail_Data.EmailSubject;

                // Opret e-mailens indhold og tilføj det til beskeden.
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.TextBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();

                // Initialiser SMTP-klienten for at sende e-mailen.
                SmtpClient MailClient = new SmtpClient();

                // Opret forbindelse til SMTP-serveren ved hjælp af værtsnavn, port og SSL-indstillinger fra MailSettings.
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);

                // Godkend med SMTP-serveren ved hjælp af e-mail-legitimationsoplysningerne fra MailSettings.
                MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);

                // Send e-mailbeskeden.
                MailClient.Send(email_Message);

                // Afbryd forbindelsen og ryd op i SMTP-klienten efter at e-mailen er sendt.
                MailClient.Disconnect(true);
                MailClient.Dispose();

                // Hvis alt lykkes, returnér true for at angive, at e-mailen blev sendt.
                return true;
            }
            catch (Exception ex)
            {
                // Hvis der opstår en fejl under afsendelse af e-mailen, returnér false.
                return false;
            }
        }
    }