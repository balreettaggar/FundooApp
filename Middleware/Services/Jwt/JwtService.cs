using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;

namespace Middleware.Service;

public class JwtService
{
   public IConfiguration configuration;
   public JwtService(IConfiguration configuration)
   {
      this.configuration = configuration;
   }

   public string GenerateToken(UserEntity user)
   {
      var claims =new []
      {
         new Claim(ClaimTypes.Email, user.Email),
         new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
      };

      var key = new SymmetricSecurityKey(
         Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
      );
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
         issuer: configuration["Jwt:Issuer"],
         audience: configuration["Jwt:Audience"],
         claims: claims,
         expires: DateTime.UtcNow.AddHours(1),
         signingCredentials: credentials
      );
      
      return new JwtSecurityTokenHandler().WriteToken(token);
   }

    public string GenerateResetToken(UserEntity user)
    {
        var claims = new[]
        {
         new Claim(
            ClaimTypes.NameIdentifier,
            user.UserId.ToString()
         ),

         new Claim(
            "purpose",
            "password-reset"
         )
      };

        var key = new SymmetricSecurityKey(
           Encoding.UTF8.GetBytes(
              configuration["Jwt:Key"]!
           )
        );

        var credentials = new SigningCredentials(
           key,
           SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
           issuer: configuration["Jwt:Issuer"],
           audience: configuration["Jwt:Audience"],
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(15),
           signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int ValidatePasswordResetToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(
           configuration["Jwt:Key"]
           ?? throw new InvalidOperationException("JWT key is not configured.")
        );

        var principal = tokenHandler.ValidateToken(
           token,
           new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,

               ValidIssuer = configuration["Jwt:Issuer"],
               ValidAudience = configuration["Jwt:Audience"],

               IssuerSigningKey = new SymmetricSecurityKey(key)
           },
           out _
        );

        var purpose = principal.FindFirst("purpose")?.Value;

        if (purpose != "password-reset")
        {
            throw new SecurityTokenException("Invalid reset token purpose.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new SecurityTokenException("Invalid user ID claim.");
        }


        return userId;
    }
}

