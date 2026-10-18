using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using NLog;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;

namespace Fundoo.Middleware;

[ApiController] // tells the application that the class is REST API controller not traditional MVC controller
// applying it provides a specific enables attribute's functionalities like if model valdidation is failed, it automatically throws
// 400 client side error 
[Route("/api/[controller]")] // this sets the url path to access this below class functionality
// route maps the url to this file (class), api is the name being contained in url, and then controller would be suffix which 
// is being replaced by the name of the class like fundoo here
public class FundooController : ControllerBase // controller base class gives us some functionalities like response.ok which then ASP.NET matches to 200
    // status code, similarly bad request 400
{
    private IUserBL userBL;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly IDistributedCache _cache;
    public FundooController(IUserBL userBL, IDistributedCache cache)
    {
        this.userBL = userBL;
        _cache = cache;
    }
    // we need dependency injection to use other class's functionalities as well, it is similar to making an instance of other class to access its functions
    // we need not to explicitly call fundooController's constructor, once api hits the controller layer, ASP .NET core resolves the dependency injection

    [HttpPost] // marks it as a post request
    [Route("register")] // resolves the base url to api/fundoo/register
    public IActionResult RegisterUser(RegistrationModel registrationModel)
    {
        ResponseModel<RegistrationModel> response =   userBL.RegisterUserBL(registrationModel); // hands control to business layer's UserBL
        if (response.IsSuccess)
        {
            logger.Info("User registeration completed succesfully");
            return Ok(response);
        } else
        {
            logger.Warn("User registeration failed");
            return BadRequest(response);
        }
    }

    [HttpPost]
    [Route("login")]
    public IActionResult LoginUserBL(LoginModel login)
    {
        ResponseModel<LoginResponseModel> response = userBL.LoginUserBL(login);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpGet("redis-test")]
    public async Task<IActionResult> RedisTest()
    {
        await _cache.SetStringAsync(
            "redis-test",
            "Fundoo connected to Redis!");

        var value = await _cache.GetStringAsync("redis-test");

        return Ok(new
        {
            message = value
        });
    }

    [HttpGet]
    public void Default()
    {
        Console.WriteLine("Application is running");
    }
    
    [Authorize]
    [HttpGet]
    [Route("profile")]
    public IActionResult GetProfile()
    {
        Console.WriteLine("Profile is displayed");
        return Ok("Profile is displayed");
    }

    [Authorize]
    [HttpPost]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordModel forgetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ForgetPasswordBL(forgetPasswordModel);
        if (responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);
    }

    [Authorize]
    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ResetPassword(resetPasswordModel);
        if (responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);
    }

    [AllowAnonymous]
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/fundoo/google-complete"
        };

        return Challenge(properties, "GoogleOIDC");
    }



    [AllowAnonymous]
    [HttpGet("google-complete")]
    public async Task<IActionResult> GoogleComplete()
    {
        var result = await HttpContext.AuthenticateAsync(
            "ExternalCookie");

        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized(new
            {
                Message = "Google sign-in failed."
            });
        }

        try
        {
            var principal = result.Principal;

            var googleSubject =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value;

            var emailVerified =
                principal.FindFirst("email_verified")?.Value;

            var firstName =
                principal.FindFirst(ClaimTypes.GivenName)?.Value
                ?? principal.FindFirst("given_name")?.Value
                ?? string.Empty;

            var lastName =
                principal.FindFirst(ClaimTypes.Surname)?.Value
                ?? principal.FindFirst("family_name")?.Value
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(googleSubject) ||
                string.IsNullOrWhiteSpace(email) ||
                !string.Equals(
                    emailVerified,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    Message =
                        "Google identity or verified email is missing."
                });
            }

            var response = userBL.GoogleLoginBL(
                email,
                googleSubject,
                firstName,
                lastName);

            if (!response.IsSuccess)
            {
                return Conflict(response);
            }

            return Ok(response);
        }
        finally
        {
            await HttpContext.SignOutAsync("ExternalCookie");
        }
    }


}

