using System.Net;
using System.Net.Mail;

namespace e_store.Services;


public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var senderEmail = _config["EmailSettings:Email"];
        var senderPassword = _config["EmailSettings:Password"];
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var mailMsg = new MailMessage()
        {
            From = new MailAddress(senderEmail, "Bis Trd"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMsg.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMsg);
    }
}