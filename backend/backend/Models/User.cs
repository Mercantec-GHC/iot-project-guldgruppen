using System.ComponentModel.DataAnnotations;

namespace backend.Models;

// Bruger-model til databasen
public class User
{
    
    public int id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public string Email { get; set; }
    
    
    // IoT-relaterede properties
    public string ArduinoId { get; set; }  
    public string PhoneNumber { get; set; }
    public bool SendEmailAlert { get; set; } = false;
    public DateTime? LastMotionAlertSentAt { get; set; }
    
    
    // Temperatur-indstillinger
    public float? TemperatureThreshold { get; set; }
    public bool SendTemperatureAlert { get; set; } = false;
    
    
    // Fugtigheds-indstillinger
    public float? HumidityThreshold { get; set; }
    public bool SendHumidityAlert { get; set; } = false;
    
    
    // Tidsstempler for sidste beskeder
    public DateTime? LastTemperatureEmailSentAt { get; set; }
    public DateTime? LastTemperatureSmsSentAt { get; set; }
    public DateTime? LastHumidityEmailSentAt { get; set; }
    public DateTime? LastHumiditySmsSentAt { get; set; }

}

// Data Transfer Object (DTO) til brugere - uden følsomme data
public class UserDTO
{
    
    public int id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    
    public string ArduinoId { get; set; }  
    public string PhoneNumber { get; set; }
    public bool SendEmailAlert { get; set; }
    
    public float? TemperatureThreshold { get; set; }
    public bool SendTemperatureAlert { get; set; }
    public float? HumidityThreshold { get; set; }
    public bool SendHumidityAlert { get; set; }
}

// DTO til registrering af nye brugere
public class UserDtoRegister
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ArduinoId { get; set; }
    public string PhoneNumber { get; set; }
}

// DTO til login
public class UserDtoLogin
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// DTO til opdatering af e-mail
public class UserDtoUpdateEmail
{
    public string NewEmail { get; set; }
    public string CurrentPassword { get; set; }
}

// DTO til opdatering af generelle alarmindstillinger
public class UpdateAlertsDto
{
    public bool SendEmailAlert { get; set; }
}

// DTO til opdatering af temperatur-alarmindstillinger
public class UpdateTemperatureAlertDto
{
    public bool SendTemperatureAlert { get; set; }
    public float? TemperatureThreshold { get; set; }
}

// DTO til opdatering af fugtigheds-alarmindstillinger
public class UpdateHumidityAlertDto
{
    public bool SendHumidityAlert { get; set; }
    public float? HumidityThreshold { get; set; }
}