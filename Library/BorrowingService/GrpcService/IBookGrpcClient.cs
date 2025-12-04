namespace BorrowingService.GrpcService;

public interface IBookGrpcClient
{
    CheckBookResult CheckAvailability(int bookId);
    
    bool ReserveBook(int bookId);
    
    bool ReturnBook(int bookId);
}

public class CheckBookResult 
{
    public bool IsAvailable { get; set; }
    public string Title { get; set; } = string.Empty;
}