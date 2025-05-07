using System.Security.Cryptography;
using System.Text;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(AppDbContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // POST: api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDtoRegister request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User already exists.");
        }

        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            ArduinoId = request.ArduinoId,
            PasswordHash = Convert.ToBase64String(passwordHash),
            PasswordSalt = Convert.ToBase64String(passwordSalt)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = user.id }, user);
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDtoLogin request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !VerifyPasswordHash(request.Password, 
                Convert.FromBase64String(user.PasswordHash), 
                Convert.FromBase64String(user.PasswordSalt)))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        string token = _jwtTokenService.GenerateToken(user.Email);
        return Ok(new { token = token });
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
    
    // GET: api/Auth/userid
    [Authorize]
    [HttpGet("userid")]
    public async Task<IActionResult> GetUserId()
    {
        Console.WriteLine($"Authorization header: {Request.Headers["Authorization"]}");
    
        if (User?.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("User not authenticated");
            return Unauthorized("Invalid token or user not authenticated.");
        }

        Console.WriteLine($"Authenticated user: {User.Identity.Name}");
        
        var email = User?.Identity?.Name;
        Console.WriteLine($"Email from token: {email}");

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("Invalid token or user not authenticated.");
        }

        // Find the user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        Console.WriteLine($"Found user ID: {user?.id}");

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Return the user ID
        return Ok(new { UserId = user.id });
    }
}