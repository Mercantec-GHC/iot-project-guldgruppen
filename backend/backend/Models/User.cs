using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class User
{
    
    public int id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public string Email { get; set; }
    
    public string ArduinoId { get; set; }  
    public string PhoneNumber { get; set; }
    public bool SendEmailAlert { get; set; } = false;
    public DateTime? LastMotionAlertSentAt { get; set; }
}

public class UserDTO
{
    
    public int id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    
    public string ArduinoId { get; set; }  
    public string PhoneNumber { get; set; }
    public bool SendEmailAlert { get; set; }
}

public class UserDtoRegister
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ArduinoId { get; set; }
    public string PhoneNumber { get; set; }
}

public class UserDtoLogin
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserDtoUpdateEmail
{
    public string NewEmail { get; set; }
    public string CurrentPassword { get; set; }
}

public class UpdateAlertsDto
{
    public bool SendEmailAlert { get; set; }
}
