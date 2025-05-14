using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
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
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUserById(int id)
    {
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
            return NotFound();
        }

        return Ok(user);
    }
    
    [HttpPut("{id}/set-alerts")]
    public async Task<IActionResult> UpdateUserAlerts(int id, [FromBody] UpdateAlertsDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.SendEmailAlert = updateDto.SendEmailAlert;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}