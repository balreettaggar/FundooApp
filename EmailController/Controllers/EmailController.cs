using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmailController.Controller;

using EmailService.Interface;
using System.Reflection.Metadata.Ecma335;
using EmailModel.Model;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    IEmailService emailService;
    
    public EmailController(IEmailService emailService) {
        this.emailService = emailService;
    }

    [HttpPost]
    [Route("send")]
    public async Task<IActionResult> SendEmail(EmailModel emailModel)
    {
        Task result = emailService.SendEmail(emailModel);

        await result;

        return Ok("Email sent succesful");
    }
    [HttpGet]
    public IActionResult DefaultRoute()
    {
        return Ok("Email project is running");
    }

}
