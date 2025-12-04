using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Library System", smtpSettings["Username"]));
        message.To.Add(new MailboxAddress("Library User", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            // Connect to the server, using the port and enabling STARTTLS
            await client.ConnectAsync(smtpSettings["Host"], int.Parse(smtpSettings["Port"]!), SecureSocketOptions.StartTls);

            // Authenticate
            await client.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);

            // Send the message
            await client.SendAsync(message);
            
            Console.WriteLine($"--> Email sent successfully to {to}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Failed to send email to {to}. Error: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
