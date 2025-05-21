using System.Security.Cryptography;
using System.Text;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")] // Basisrute for alle endpoints i denne controller
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context; // Databasekontekst
    private readonly JwtTokenService _jwtTokenService; // Service til JWT-token håndtering

    // Constructor med dependency injection
    public AuthController(AppDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // POST: api/Auth/register //HER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDtoRegister request)
    {
        // Tjek om bruger allerede eksisterer
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User already exists.");
        }

        // Generer password hash og salt
        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        // Opret ny bruger
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            ArduinoId = request.ArduinoId,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = Convert.ToBase64String(passwordHash), // Gem som Base64 string
            PasswordSalt = Convert.ToBase64String(passwordSalt) // Gem som Base64 string
        };

        // Tilføj bruger til databasen
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Returner 201 Created med brugeroplysninger
        return CreatedAtAction(nameof(Register), new { id = user.id }, user);
    }

    // POST: api/Auth/login  //HER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDtoLogin request)
    {
        // Find bruger baseret på email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        // Tjek om bruger findes og password er korrekt
        if (user == null || !VerifyPasswordHash(request.Password, 
                Convert.FromBase64String(user.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt)))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        // Generer JWT token
        string token = _jwtTokenService.GenerateToken(user.Email);
        return Ok(new { token = token });
    }

    // Hjælpemetode til at generere password hash og salt
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key; // Gem nøglen som salt
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Beregn hash
        }
    }

    // Hjælpemetode til at verificere password hash
    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash); // Sammenlign hashes
        }
    }
    
    // GET: api/Auth/userid
    [Authorize] // Kræver autentificering
    [HttpGet("userid")]
    public async Task<IActionResult> GetUserId()
    {
        Console.WriteLine($"Authorization header: {Request.Headers["Authorization"]}");
    
        // Tjek om bruger er autentificeret
        if (User?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("User not authenticated");
            return Unauthorized("Invalid token or user not authenticated.");
        }

        Console.WriteLine($"Authenticated user: {User.Identity.Name}");
        
        // Hent email fra JWT token
        var email = User?.Identity?.Name;
        Console.WriteLine($"Email from token: {email}");

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("Invalid token or user not authenticated.");
        }

        // Find bruger i databasen
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        Console.WriteLine($"Found user ID: {user?.id}");

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Returner brugerens ID
        return Ok(new { UserId = user.id });
    }
    
    // POST: api/Auth/update-email
    [Authorize] // Kræver autentificering
    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UserDtoUpdateEmail request)
    {
        // Hent nuværende email fra JWT token
        var currentEmail = User?.Identity?.Name;
        if (string.IsNullOrEmpty(currentEmail))
        {
            return Unauthorized("Invalid token or user not authenticated.");
        }

        // Find bruger i databasen
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentEmail);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Verificer at det nuværende password er korrekt
        if (!VerifyPasswordHash(request.CurrentPassword, 
                Convert.FromBase64String(user.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt)))
        {
            return Unauthorized("Current password is incorrect.");
        }

        // Tjek om den nye email allerede er i brug
        if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail))
        {
            return BadRequest("The new email is already in use.");
        }

        // Opdater email
        user.Email = request.NewEmail;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Generer nyt JWT token med den opdaterede email
        string token = _jwtTokenService.GenerateToken(user.Email);

        return Ok(new { 
            message = "Email updated successfully.",
            token = token // Returner nyt token til klienten
        });
    }
}