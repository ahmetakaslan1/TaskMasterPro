using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskMasterPro.IntegrationTests.Helpers;

public static class JwtTokenHelper
{
    public static string GenerateTestToken(Guid userId, string email = "test@test.com")
    {
        var secret = "this_is_a_super_secret_key_for_testing_purposes_only"; // Must match the factory env variable
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: "TaskMasterPro",
            audience: "TaskMasterProClients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
