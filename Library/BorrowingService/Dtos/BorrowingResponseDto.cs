namespace BorrowingService.Dtos;

public class BorrowingResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
    
    public List<BorrowingItemDto> Items { get; set; }
}