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
using Microsoft.Extensions.Caching.Distributed;
using Middleware.Service;
using Middleware.Services.Encryption;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    // These fields hold references to objects that the business layer may use.
    public IUserRL userRL; // for repository layer, calls RL
    public JwtService jwtService; // handles jwt related functionality
    public EmailClient client; // used for email sending functionality
    public IConfiguration configuration; // mostly for user secrets or env file
    private readonly IDistributedCache _cache; // provides access to redis cache
    private readonly PasswordHandler passwordService; // for our custom salt maker, hashes the password and verifies
    public UserBL(IUserRL userRL , JwtService jwtService, EmailClient client, IConfiguration configuration, IDistributedCache _cache, PasswordHandler passwordService)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
        this.configuration = configuration;
        this._cache = _cache;
        this.passwordService = passwordService;
    }

    public ResponseModel<string> RegisterUserBL(RegistrationModel model)
    {
        var (hash, salt) = passwordService.HashPassword(model.Password);
        // model.password is the plain text arrived with the http request, we are forwarding it to passwordHandler's instance
        // we are catching hash and salt being returned
        // The (hash, salt) syntax is called tuple deconstruction. Your method returns two strings bundled together
        // in a tuple, and this line unpacks them into two local variables.

        var user = new UserEntity
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.ContactNumber,
            Password = hash,
            Salt = salt
        };

        userRL.RegisterUserRL(user);

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Registration successful.",
            Data = user.Email
        };
    }

    //public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    //{

    //    UserEntity? userEntity =   userRL.LoginUserRL(login);
    //    ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
    //    if (userEntity == null)
    //    {
    //        rm.IsSuccess = false;
    //        rm.Message = "login email or password is wrong";
    //        rm.Data = null;
    //    }
    //    else
    //    {
    //        rm.IsSuccess = true;
    //        rm.Message = "login success";
    //        rm.Data = new LoginResponseModel
    //        {
    //            Email = userEntity.Email,
    //            Token = jwtService.GenerateToken(userEntity)
    //        };
    //    }
    //    return rm;
    //}

    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel loginModel)
    {
        UserEntity? user = userRL.FindUserByEmail(loginModel.Email);

        if (user == null)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        bool isPasswordValid = passwordService.VerifyPassword(
            loginModel.Password,
            user.Password,
            user.Salt
        );

        if (!isPasswordValid)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        return new ResponseModel<LoginResponseModel>
        {
            IsSuccess = true,
            Message = "Login successful.",
            Data = new LoginResponseModel
            {
                Email = user.Email,
                Token = jwtService.GenerateToken(user)
            }
        };
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


    public ResponseModel<LoginResponseModel> GoogleLoginBL(
        string email,
        string providerSubject,
        string firstName,
        string lastName)
    {
        try
        {
            UserEntity? user = userRL.FindUserByExternalLogin(
                "Google",
                providerSubject);

            if (user == null)
            {
                user = userRL.CreateGoogleUserWithExternalLogin(
                    email,
                    firstName,
                    lastName,
                    providerSubject);
            }

            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = true,
                Message = "Google login successful.",
                Data = new LoginResponseModel
                {
                    Email = user.Email,
                    Token = jwtService.GenerateToken(user)
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = ex.Message,
                Data = null
            };
        }

    }

}