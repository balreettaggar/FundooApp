using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Fundoo.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FundooController : ControllerBase
{
    private IUserBL userBL;
    public FundooController(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    [HttpPost]
    [Route("register")]
    public IActionResult RegisterUser(RegistrationModel registrationModel)
    {
        ResponseModel<RegistrationModel> response =   userBL.RegisterUserBL(registrationModel);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
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
    
}

