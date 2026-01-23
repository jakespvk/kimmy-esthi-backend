using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace KimmyEsthi.Email;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendAppointmentRequestEmail(Guid clientId, Guid? appointmentId, string clientEmail)
    {
        try
        {
            var credentials = GetEmailCredentials();

            var subject = "Appointment Request Confirmation";
            var body = $"""
                Your appointment request has been successfully created! Thank you for trusting SunsetKimcare with your skin :)
                If you have not done so already, please fill out a consent form <a href="https://sunsetkimcare.com/booking/scheduleAppointment/consentForm?appointmentId={appointmentId}&clientId={clientId}">here.</a>
                """;

            return await SendEmailAsync(credentials.Item1, credentials.Item2, clientEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment request email");
            return false;
        }
    }

    private (string, string) GetEmailCredentials()
    {
        var username = Environment.GetEnvironmentVariable("GMAIL_USERNAME")!;
        var password = Environment.GetEnvironmentVariable("GMAIL_PASSWORD")!;
        return (username, password);
    }

    private async Task<bool> SendEmailAsync(string username, string password, string toEmail, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Kimmy Esthi", username));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }
}
