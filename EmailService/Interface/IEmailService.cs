namespace EmailService.Interface;
using EmailModel.Model;
public interface IEmailService
{
    Task SendEmail(EmailModel email);
}