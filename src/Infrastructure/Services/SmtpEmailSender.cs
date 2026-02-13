using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class SmtpEmailSender(
    IConfiguration configuration, 
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587");
        var username = configuration["EmailSettings:SmtpUsername"];
        var password = configuration["EmailSettings:SmtpPassword"];
        var senderEmail = configuration["EmailSettings:SenderEmail"] ?? "no-reply@cats.com";
        var senderName = configuration["EmailSettings:SenderName"] ?? "CATS System";

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(username))
        {
            logger.LogWarning("Email settings not configured. Skipping email to {To}", to);
            return;
        }

        try
        {
            using var client = new SmtpClient(smtpServer, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(username, password);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}
