using System.Net;
using System.Text;
using System.Net.Mail;

namespace gupyScraper
{
    public class Worker : BackgroundService
    {
        private readonly GupyScraper _scraper;
        private readonly ILogger<Worker> _logger;
        private readonly string _yourEmailAddress = "seu_email";
        private readonly string _yourAppPassword = "token_password";
        private List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)> _lastSentJobData = new List<(string, string, DateTime, string)>();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            _scraper = new GupyScraper(
                ["Analista de Sistemas",
                "Desenvolvedor de Software",
                "Engenheiro de Software",
                "Programador",
                "Desenvolvedor Full Stack",
                "Desenvolvedor .NET Core",
                "Desenvolvedor .NET",
                "Desenvolvedor C#",
                "Desenvolvedor C# Junior",
                "Desenvolvedor C# Pleno",
                "Desenvolvedor C# Senior",
                "Desenvolvedor Front End",
                "Front End Developer",
                "FullStack",
                "Node.js Developer",
                "Full Stack Pleno",
                ".NET Core",
                ".NET Developer",
                "Full Stack Junior",
                "C# Júnior",
                "Desenvolvedor .NET Junior",
                "Desenvolvedor .NET Pleno",
                "Desenvolvedor de Aplicações .NET",
                "Engenheiro de Software .NET",
                "Engenheiro Full Stack",
                "Engenheiro de Software C#",
                "Programador .NET",
                "Programador C#",
                "Analista Desenvolvedor .NET",
                "Analista de Sistemas .NET",
                "Full Stack Node.js Developer",
                "Full Stack Next.js Developer",
                "Full Stack JavaScript Developer",
                "Programador Full Stack Node.js",
                "Programador Full Stack Next.js",
                "Desenvolvedor Full Stack Angular",
                "Engenheiro de Software Full Stack Angular",
                "Programador Full Stack Angular",
                "Desenvolvedor Angular",
                "Desenvolvedor Node.js",
                "Desenvolvedor Next.js",
                "Engenheiro de Software Node.js",
                "Engenheiro de Software Next.js",
                "Programador Node.js",
                "Programador Next.js",
                "Estágio Desenvolvedor Full Stack",
                "Estágio Desenvolvedor .NET",
                "Estágio Desenvolvedor C#",
                "Estágio Desenvolvedor Node.js",
                "Estágio Desenvolvedor Angular",
                "Estágio Desenvolvedor Next.js",
                "Estágio Desenvolvedor Front End",
                "Estágio Full Stack Developer",
                "Estágio Node.js Developer",
                "Estágio Next.js Developer",
                "Estágio Angular Developer",
                "Estágio .NET Developer",
                "Estágio C# Developer"]
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jobData = await _scraper.RequestJobDataAsync();

                    var orderedJobData = jobData.OrderByDescending(job => job.datePublished).ToList();

                    if (!ListsAreEqual(orderedJobData, _lastSentJobData))
                    {
                        _lastSentJobData = orderedJobData;
                        var emailBody = GenerateEmailTemplate(orderedJobData);
                        SendEmail(emailBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao salvar arquivo: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromHours(1   ), stoppingToken);
            }
        }

        private bool ListsAreEqual(List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)> list1, List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i].JobTitle != list2[i].JobTitle || list1[i].JobUrl != list2[i].JobUrl || list1[i].datePublished != list2[i].datePublished || list1[i].workPlaceType != list2[i].workPlaceType)
                    return false;
            }

            return true;
        }

        private string GenerateEmailTemplate(List<(string JobTitle, string JobUrl, DateTime datePublished, string workPlaceType)> jobData)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendLine("<html>");
            emailBody.AppendLine("<head>");
            emailBody.AppendLine("<style>");
            emailBody.AppendLine("body { font-family: 'Roboto', sans-serif; background-color: #ffffff; color: #333333; margin: 0; padding: 20px; }");
            emailBody.AppendLine(".container { padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }");
            emailBody.AppendLine("h1 { color: #1976d2; text-align: center; margin-bottom: 20px; }");
            emailBody.AppendLine(".job-table { width: 100%; border-collapse: collapse; }");
            emailBody.AppendLine(".job-table th, .job-table td { padding: 10px; text-align: left; border: 1px solid #e0e0e0; }");
            emailBody.AppendLine(".job-table th { background-color: #f5f5f5; }");
            emailBody.AppendLine(".job-link { color: #1976d2; text-decoration: none; }");
            emailBody.AppendLine(".job-link:hover { text-decoration: underline; }");
            emailBody.AppendLine("</style>");
            emailBody.AppendLine("</head>");
            emailBody.AppendLine("<body>");
            emailBody.AppendLine("<div class='container'>");
            emailBody.AppendLine("<h1>Lista de Vagas Disponíveis</h1>");
            emailBody.AppendLine("<table class='job-table'>");
            emailBody.AppendLine("<thead>");
            emailBody.AppendLine("<tr>");
            emailBody.AppendLine("<th>Título</th>");
            emailBody.AppendLine("<th>Data de Publicação</th>");
            emailBody.AppendLine("<th>Modelo de Trabalho</th>");
            emailBody.AppendLine("<th>Ver Vaga</th>");
            emailBody.AppendLine("</tr>");
            emailBody.AppendLine("</thead>");
            emailBody.AppendLine("<tbody>");

            foreach (var job in jobData)
            {
                emailBody.AppendLine("<tr>");
                emailBody.AppendLine($"<td>{job.JobTitle}</td>");
                emailBody.AppendLine($"<td>{job.datePublished:dd-MM-yyyy}</td>");
                emailBody.AppendLine($"<td>{job.workPlaceType}</td>");
                emailBody.AppendLine($"<td><a class='job-link' href='{job.JobUrl}'>Ver Vaga</a></td>");
                emailBody.AppendLine("</tr>");
            }

            emailBody.AppendLine("</tbody>");
            emailBody.AppendLine("</table>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("</body>");
            emailBody.AppendLine("</html>");

            return emailBody.ToString();
        }

        private void SendEmail(string emailBody)
        {
            try
            {
                var fromAddress = new MailAddress(_yourEmailAddress, "Seu nome");
                var toAddress = new MailAddress(_yourEmailAddress, "Seu nome");
                const string subject = "Lista de Vagas";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, _yourAppPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);

                _logger.LogInformation("Email enviado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao enviar email: {ex.Message}");
            }
        }
    }
}
