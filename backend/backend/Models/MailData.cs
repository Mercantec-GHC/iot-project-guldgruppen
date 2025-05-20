namespace backend.Models;

// Model til at indeholde data for e-mails der skal sendes
public class MailData
{
    // Modtagerens e-mailadresse
    public string EmailToId { get; set; }
    
    // Modtagerens navn (valgfri - bruges til personalisering)
    public string EmailToName { get; set; }
    
    // Emne for e-mailen
    public string EmailSubject { get; set; }
    
    // Indholdet af e-mailen (brødtekst)
    public string EmailBody { get; set; }
}