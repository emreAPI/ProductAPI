using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;

[Route("api/token")]
[ApiController]
public class TokenController : ControllerBase
{
    [HttpPost]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (!ValidateClient(request.ClientId, request.ClientSecret))
        {
            return Unauthorized(); 
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Username")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-long-secret-key-256-bits"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7265",
            audience: "https://localhost:7265",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }

    private bool ValidateClient(string clientId, string clientSecret)
    {
        return clientId == "your-client-id" && clientSecret == "your-long-secret-key-256-bits";
    }
}
