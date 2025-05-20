namespace backend.Configuration;

// Konfigurationsklasse til e-mail-indstillinger
// Bruges til at indlæse indstillinger fra appsettings.json
public class MailSettings
{
    // Afsenderens e-mailadresse
    public string EmailId { get; set; }
    
    // Afsenderens navn (vises i modtagerens e-mailklient)
    public string Name { get; set; }
    
    // Brugernavn til SMTP-autentificering
    public string UserName { get; set; }
    
    // Adgangskode til SMTP-autentificering
    public string Password { get; set; }
    
    // SMTP-serverens hostname (f.eks. 'smtp.leeloo.dk')
    public string Host { get; set; }
    
    // Portnummer til SMTP-serveren (typisk 587 for TLS)
    public int Port { get; set; }
    
    // Angiver om SSL skal bruges til forbindelsen
    public bool UseSSL { get; set; }
}