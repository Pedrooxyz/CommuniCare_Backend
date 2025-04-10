namespace CommuniCare
{
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using System.Net.Mail;
    using System.Net;

    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            var client = new SendGridClient(_configuration["SendGridSettings:ApiKey"]);
            var from = new EmailAddress(_configuration["SendGridSettings:FromEmail"]);
            var subject = "Recuperação de Senha";
            var to = new EmailAddress(email);
            var plainTextContent = $"Clique no link abaixo para redefinir sua senha:\n{resetLink}";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, plainTextContent);

            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // Trate a falha ao enviar o e-mail, talvez lançando um erro
                throw new Exception("Erro ao enviar o e-mail");
            }
        }
    }



}
