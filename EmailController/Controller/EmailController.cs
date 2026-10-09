using EmailService.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using EmailService.Service;
using EmailService.Interface;

namespace EmailController.Controller;
using EmailModel.Model;
[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    IEmailService emailService;

    public EmailController(IEmailService emailService)
    {
        this.emailService = emailService;
    }
    
    [HttpGet]
    public IActionResult defaultRoute()
    {
        return Ok("Email project is running");
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail(EmailModel model)
    {
        await emailService.SendEmail(model);

        return Ok("Email sent successfully");
    }
}