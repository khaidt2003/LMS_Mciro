using NotificationService;
using NotificationService.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Register the Email Service as a Singleton
        services.AddSingleton<IEmailService, EmailService>();
        
        // Register the Worker which listens to RabbitMQ
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();