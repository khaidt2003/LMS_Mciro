namespace BorrowingService.QueueMessageService;

public interface IMessageProducer
{
    Task SendMessageAsync<T>(T message, string routingKey);
}   