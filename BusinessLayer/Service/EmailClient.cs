using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EmailModel.Model;
using EmailService.Interface;

namespace BusinessLayer.Service
{
    public class EmailClient
    {
        private readonly IEmailService emailService;

        public EmailClient( IEmailService emailService)
        {
            this.emailService = emailService;  
        }

        public async Task<bool> SendEmail(EmailModel.Model.EmailModel model)
        {
            await emailService.SendEmail(model);
            return true;
        }
    }
}
