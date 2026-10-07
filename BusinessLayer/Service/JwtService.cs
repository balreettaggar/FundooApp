using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;

namespace BusinessLayer.Service;

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
}