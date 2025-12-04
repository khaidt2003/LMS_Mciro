using System.Text;
using System.Text.Json;
using NotificationService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService;

public class NotificationEventDto
{
    public string? Event { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public string? Title { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    
    private const string ExchangeName = "library_exchange";
    private const string QueueName = "notification_queue";

    public Worker(
        ILogger<Worker> logger, 
        IEmailService emailService, 
        IConfiguration configuration)
    {
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Hostname"],
            Port = int.Parse(_configuration["RabbitMQ:Port"]!),
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"],
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = await factory.CreateConnectionAsync(stoppingToken);
                using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Khai báo exchange + queue + binding
                await channel.ExchangeDeclareAsync(
                    exchange: ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: "email.borrow",
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: "email.return",
                    arguments: null,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Information: RabbitMQ connection established. Waiting for messages.");

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Information: Received message: {Message}", message);

                    try
                    {
                        var notificationEvent = JsonSerializer.Deserialize<NotificationEventDto>(
                            message,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (notificationEvent == null)
                        {
                            _logger.LogWarning("--> Received a message that could not be deserialized.");
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                            return;
                        }

                        string recipientEmail = _configuration["SmtpSettings:Username"]!;
                        string emailSubject;
                        string emailBody;

                        if (notificationEvent.Event == "BookBorrowed")
                        {
                            emailSubject = "Book Borrowing Confirmation";
                            emailBody =
                                $"<h1>Your book borrowing is confirmed!</h1>" +
                                $"<p>Dear User {notificationEvent.UserId},</p>" +
                                $"<p>You have borrowed the book: <strong>{notificationEvent.Title}</strong> (ID: {notificationEvent.BookId}).</p>" +
                                $"<p>Please return it by: <strong>{notificationEvent.DueDate:yyyy-MM-dd}</strong>.</p>" +
                                "<p>Thank you for using our library!</p>";
                        }
                        else if (notificationEvent.Event == "BookReturned")
                        {
                            emailSubject = "Book Return Confirmation";
                            emailBody =
                                $"<h1>Your book return is confirmed!</h1>" +
                                $"<p>Dear User {notificationEvent.UserId},</p>" +
                                $"<p>You have returned the book: <strong>{notificationEvent.Title}</strong> (ID: {notificationEvent.BookId}).</p>" +
                                $"<p>Return date: <strong>{notificationEvent.ReturnDate:yyyy-MM-dd HH:mm}</strong>.</p>" +
                                "<p>We look forward to seeing you again!</p>";
                        }
                        else
                        {
                            _logger.LogWarning("Warning: Unknown event type '{EventType}'. Ignoring message.", notificationEvent.Event);
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                            return;
                        }

                        await _emailService.SendEmailAsync(recipientEmail, emailSubject, emailBody);

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error: Error processing message.");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                // Chờ vô hạn tới khi bị cancel (dịch vụ bị stop)
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Information: Worker is shutting down.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: Could not connect to RabbitMQ. Retrying in 5 seconds... Message: {Message}", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
