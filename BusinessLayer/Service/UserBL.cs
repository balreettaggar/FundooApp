using BusinessLayer.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    public IUserRL userRL;
    public JwtService jwtService;
    public EmailClient client;
    public IConfiguration configuration;
    public UserBL(IUserRL userRL , JwtService jwtService, EmailClient client, IConfiguration configuration)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
        this.configuration = configuration;
    }
    
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    {
        return userRL.RegisterUserRL(register);
    }

    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    {
        
        UserEntity? userEntity =   userRL.LoginUserRL(login);
        ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
        if (userEntity == null)
        {
            rm.IsSuccess = false;
            rm.Message = "login email or password is wrong";
            rm.Data = null;
        }
        else
        {
            rm.IsSuccess = true;
            rm.Message = "login success";
            rm.Data = new LoginResponseModel
            {
                Email = userEntity.Email,
                Token = jwtService.GenerateToken(userEntity)
            };
        }
        return rm;
    }

    public async Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model)
    {
        UserEntity user = userRL.FindUserByEmail(model.Email);

        if (user == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            };
        }

        var token = jwtService.GenerateResetToken(user);

        var emailModel = new EmailModel.Model.EmailModel()
        {
            To = user.Email,
            Subject = "Fundoo Password Reset",
            Body = $"""
                    Your password reset token is:
                    {token}
                    This token will expire in 15 minutes.
                    """
        };
        await client.SendEmail(emailModel);
        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset.",
            Data = token
        };

    }

    public async Task<ResponseModel<string>> ResetPassword(
        ResetPasswordModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            };
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"]
        );

        try
        {
            var principal = tokenHandler.ValidateToken(
                model.Token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(key)
                },
                out SecurityToken validatedToken
            );

            var purpose = principal.FindFirst("purpose")?.Value;

            if (purpose != "password-reset")
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "Invalid reset token.",
                    Data = null
                };
            }

            var userIdClaim =
                principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "Invalid reset token.",
                    Data = null
                };
            }

            int userId = int.Parse(userIdClaim.Value);

            UserEntity? user = userRL.GetUserById(userId);

            if (user == null)
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            var passwordHasher = new PasswordHasher<UserEntity>();

            user.Password = passwordHasher.HashPassword(
                user,
                model.NewPassword
            );

            userRL.UpdatePassword(user);

            return new ResponseModel<string>
            {
                IsSuccess = true,
                Message = "Your password has been reset successfully.",
                Data = null
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            };
        }
        catch (SecurityTokenException)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            };
        }
    }
}