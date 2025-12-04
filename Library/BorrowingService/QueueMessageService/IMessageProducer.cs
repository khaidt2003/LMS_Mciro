namespace BorrowingService.QueueMessageService;

public interface IMessageProducer
{
    // Đổi từ void sang Task để hỗ trợ await
    Task SendMessageAsync<T>(T message, string routingKey);
}   