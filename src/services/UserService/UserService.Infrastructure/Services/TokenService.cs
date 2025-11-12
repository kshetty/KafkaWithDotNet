using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Common.Interfaces.Services;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public sealed class TokenService : ITokenService
{
  private readonly IConfiguration _configuration;
  private readonly SymmetricSecurityKey _signingKey;

  public TokenService(IConfiguration configuration)
  {
    _configuration = configuration;

    var secret = _configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey is not configured");

    _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
  }

  public string GenerateAccessToken(Guid userId, string email, string fullName)
  {
    var claims = new[]
    {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

    var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

    var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
    var issuer = _configuration["Jwt:Issuer"] ?? "UserService";
    var audience = _configuration["Jwt:Audience"] ?? "UserServiceClient";

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public bool ValidateToken(string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var issuer = _configuration["Jwt:Issuer"] ?? "UserService";
      var audience = _configuration["Jwt:Audience"] ?? "UserServiceClient";

      var validationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = _signingKey,
        ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
      };

      tokenHandler.ValidateToken(token, validationParameters, out _);
      return true;
    }
    catch
    {
      return false;
    }
  }

  private ClaimsPrincipal? GetPrincipalFromToken(string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var issuer = _configuration["Jwt:Issuer"] ?? "UserService";
      var audience = _configuration["Jwt:Audience"] ?? "UserServiceClient";

      var validationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false, // Don't validate lifetime for extraction
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = _signingKey
      };

      var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
      return principal;
    }
    catch
    {
      return null;
    }
  }

  public Guid? GetUserIdFromToken(string token)
  {
    var principal = GetPrincipalFromToken(token);
    if (principal == null)
    {
      return null;
    }

    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
        ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
    {
      return null;
    }

    return userId;
  }
}
