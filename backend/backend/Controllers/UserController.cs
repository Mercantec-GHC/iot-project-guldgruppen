using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")] // Basisrute: /api/users
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context; // Databasekontekst til brugerhåndtering

    // Constructor med dependency injection af databasekontekst
    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    // Henter alle brugere (uden følsomme data som passwords)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
        // Henter og projicerer brugere til DTO-format
        var users = await _context.Users
            .Select(u => new UserDTO
            {
                id = u.id,
                Username = u.Username,
                Email = u.Email,
                ArduinoId = u.ArduinoId
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/Users/{id}
    // Henter en specifik bruger ud fra ID
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUserById(int id)
    {
        // Finder bruger og projicerer til DTO
        var user = await _context.Users
            .Where(u => u.id == id)
            .Select(u => new UserDTO
            {
                id = u.id,
                Username = u.Username,
                Email = u.Email,
                ArduinoId = u.ArduinoId
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(); // 404 hvis bruger ikke findes
        }

        return Ok(user);
    }
    
    // PUT: api/Users/{id}/set-alerts
    // Opdaterer brugerens generelle alarmindstillinger
    [HttpPut("{id}/set-alerts")]
    public async Task<IActionResult> UpdateUserAlerts(int id, [FromBody] UpdateAlertsDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Opdaterer kun det relevante felt
        user.SendEmailAlert = updateDto.SendEmailAlert;
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // PUT: api/Users/{id}/set-temperature-alerts
    // Opdaterer brugerens temperatur-alarmindstillinger
    [HttpPut("{id}/set-temperature-alerts")]
    public async Task<IActionResult> UpdateTemperatureAlerts(int id, [FromBody] UpdateTemperatureAlertDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Opdaterer begge temperatur-relaterede felter
        user.SendTemperatureAlert = updateDto.SendTemperatureAlert;
        user.TemperatureThreshold = updateDto.TemperatureThreshold;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Users/{id}/set-humidity-alerts
    // Opdaterer brugerens fugtigheds-alarmindstillinger
    [HttpPut("{id}/set-humidity-alerts")]
    public async Task<IActionResult> UpdateHumidityAlerts(int id, [FromBody] UpdateHumidityAlertDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Opdaterer begge fugtigheds-relaterede felter
        user.SendHumidityAlert = updateDto.SendHumidityAlert;
        user.HumidityThreshold = updateDto.HumidityThreshold;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}