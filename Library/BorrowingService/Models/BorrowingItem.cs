using System.ComponentModel.DataAnnotations.Schema;

namespace BorrowingService.Models;

public class BorrowingItem
{
    public int Id { get; set; }
    
    public int BorrowerId { get; set; }
    [ForeignKey(nameof(BorrowerId))] public Borrowing Borrowing { get; set; } = null!;
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = "Borrowed";
}