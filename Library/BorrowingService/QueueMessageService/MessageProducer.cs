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
                // CreateConnectionAsync thay vì CreateConnection
                _connection = await _factory.CreateConnectionAsync();
                
                // CreateChannelAsync thay vì CreateModel
                _channel = await _connection.CreateChannelAsync();

                // ExchangeDeclareAsync
                await _channel.ExchangeDeclareAsync(exchange: "library_exchange", type: ExchangeType.Direct);

                Console.WriteLine("--> Kết nối RabbitMQ (Async) thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Can not Connect RabbitMQ: {ex.Message}");
            }
        }

        public async Task SendMessageAsync<T>(T message, string routingKey)
        {
            // Kiểm tra: Nếu chưa kết nối hoặc kết nối bị đóng thì kết nối lại
            if (_connection == null || !_connection.IsOpen)
            {
                await InitRabbitMQ();
            }

            // Nếu vẫn không kết nối được thì bỏ qua (hoặc throw exception tùy bạn)
            if (_channel == null || !_channel.IsOpen) 
            {
                 Console.WriteLine("--> RabbitMQ chưa sẵn sàng.");
                 return;
            }

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // BasicPublishAsync thay vì BasicPublish
            await _channel.BasicPublishAsync(
                exchange: "library_exchange",
                routingKey: routingKey,
                mandatory: false, // Thêm tham số này (mặc định false)
                basicProperties: new BasicProperties(), // Cần khởi tạo properties
                body: body);

            Console.WriteLine($"--> Đã gửi tin nhắn (Async): {json}");
        }
    }
}