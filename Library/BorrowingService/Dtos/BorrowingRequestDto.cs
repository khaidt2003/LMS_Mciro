namespace BorrowingService.Dtos;

public class BorrowingRequestDto
{
    public DateTime DueDate { get; set; }
    
    public List<BorrowingItemDto> BorrowItems { get; set; } = new List<BorrowingItemDto>();
}