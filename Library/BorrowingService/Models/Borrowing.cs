using System.ComponentModel.DataAnnotations.Schema;

namespace BorrowingService.Models;

public class Borrowing
{
    public  int Id { get; set; }
    
    public int UserId { get; set; }
    
    public DateTime BorrowDate { get; set; }
    
    public DateTime BorrowedDate { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public string Status { get; set; }
    
    public ICollection<BorrowingItem> BorrowingItems { get; set; } = new List<BorrowingItem>();
    
}