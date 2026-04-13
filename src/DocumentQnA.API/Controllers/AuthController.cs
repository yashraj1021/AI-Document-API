using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentQnA.Contracts.Requests;
using DocumentQnA.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DocumentQnA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    // Hardcoded test users - fine for development/demo purposes
    private static readonly Dictionary<string, string> TestUsers = new()
    {
        { "testuser", "password123" },
        { "admin", "admin123" }
    };

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validate user
        if (!TestUsers.TryGetValue(request.Username, out var expectedPassword)
            || expectedPassword != request.Password)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        var token = GenerateJwtToken(request.Username);
        return Ok(token);
    }

    private LoginResponse GenerateJwtToken(string username)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "";
        var issuer = _configuration["Jwt:Issuer"] ?? "";
        var audience = _configuration["Jwt:Audience"] ?? "";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = username,
            ExpiresAt = expiresAt
        };
    }
}