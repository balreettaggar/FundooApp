using EmailService.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace EmailService.Service;

public class EmailService : IEmailService
{


    private readonly IConfiguration configuration;

    public EmailService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task SendEmail(EmailModel.Model.EmailModel email)
    {
        
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                "Fundoo App",
                configuration["SmtpSettings:Username"]
            )
        );

        message.To.Add(
            MailboxAddress.Parse(email.To)
        );

        //message : ->
        //To
        //From
        //Subject
        //Body
        message.Subject = email.Subject;
        message.Body = new TextPart("html")
        {
            Text = email.Body
        };

        using var smtp = new SmtpClient();
        smtp.CheckCertificateRevocation = false;
        await smtp.ConnectAsync(
            configuration["SmtpSettings:Host"],
            int.Parse(configuration["SmtpSettings:Port"]!),
            SecureSocketOptions.StartTls
        );
        
        await smtp.AuthenticateAsync(
            configuration["SmtpSettings:Username"],
            configuration["SmtpSettings:Password"]
        );
        
        await smtp.SendAsync(message);

        await smtp.DisconnectAsync(true);
    
        
    }
}