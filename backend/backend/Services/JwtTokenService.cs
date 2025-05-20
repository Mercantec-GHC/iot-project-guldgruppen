using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

// Service til håndtering af JWT (JSON Web Token) generation
public class JwtTokenService
{
    private readonly IConfiguration _configuration; // Tilgang til app-indstillinger

    // Constructor med dependency injection af konfiguration
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Generer en JWT token for den angivne email
    public string GenerateToken(string email)
    {
        // Opret claims (informationer der indgår i token)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email), // Brugerens email som hovedclaim
        };

        // Hent den hemmelige nøgle fra konfiguration
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
        
        // Opret signaturcredentials med nøgle og algoritme
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // Opret selve token med:
        var token = new JwtSecurityToken(
            claims: claims, // Brugerens claims
            expires: DateTime.Now.AddDays(1), // Udløbstid (1 dag)
            signingCredentials: creds); // Signaturinformation

        // Konverter token til en streng og returner
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}