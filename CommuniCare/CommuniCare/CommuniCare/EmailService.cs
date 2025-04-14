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


        public async Task EnviarComprovativoCompra(string email, string nome, byte[] pdfBytes)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new Exception("PDF está vazio ou inválido.");

            var apiKey = _configuration["SendGridSettings:ApiKey"];


            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("API Key do SendGrid não está configurada.");

            //if (string.IsNullOrWhiteSpace(fromEmail))
            //    throw new Exception("Email remetente do SendGrid não está configurado.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["SendGridSettings:FromEmail"]);
            var to = new EmailAddress(email, nome);
            var subject = "Comprovativo de Compra - CommuniCare";
            var plainTextContent = "Segue em anexo o comprovativo da sua compra.";
            var htmlContent = "<strong>Segue em anexo o comprovativo da sua compra.</strong>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var fileBase64 = Convert.ToBase64String(pdfBytes);
            msg.AddAttachment("comprovativo.pdf", fileBase64, "application/pdf");

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Erro ao enviar o comprovativo por email. Código: {response.StatusCode}");
        }

    }

    }
