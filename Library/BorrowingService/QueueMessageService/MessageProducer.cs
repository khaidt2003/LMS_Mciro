// AsyncDataServices/MessageProducer.cs
using System.Text;
using System.Text.Json;
using BorrowingService.QueueMessageService;
using RabbitMQ.Client; // Đảm bảo bạn đang dùng v7.x

namespace BorrowingService.AsyncDataServices
{
    public class MessageProducer : IMessageProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;

        public MessageProducer(IConfiguration configuration)
        {
            _configuration = configuration;

            // 1. Chỉ cấu hình Factory ở đây, chưa kết nối ngay
            _factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Hostname"],
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };
        }

        // Hàm khởi tạo kết nối riêng (Private)
        private async Task InitRabbitMQ()
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                
                _channel = await _connection.CreateChannelAsync();
                await _channel.ExchangeDeclareAsync(
                    exchange: "library_exchange", 
                    type: ExchangeType.Direct, 
                    durable: true 
                );

                Console.WriteLine("--> Connect RabbitMQ (Async) Sucess");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Can not Connect RabbitMQ: {ex.Message}");
            }
        }

        public async Task SendMessageAsync<T>(T message, string routingKey)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                await InitRabbitMQ();
            }
            
            if (_channel == null || !_channel.IsOpen) 
            {
                 Console.WriteLine("--> RabbitMQ not ready.");
                 return;
            }

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            await _channel.BasicPublishAsync(
                exchange: "library_exchange",
                routingKey: routingKey,
                mandatory: false,
                basicProperties: new BasicProperties(), 
                body: body);

            Console.WriteLine($"--> Send Message (Async): {json}");
        }
    }
}