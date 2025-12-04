using BorrowingService.Models;

namespace BorrowingService.Repository;

public interface IBorrowingRepository
{
    Task<Borrowing> AddBorrowingAsync(Borrowing borrowing);
    
    Task<BorrowingItem?> GetItemByIdAsync(int itemId);
    
    Task<bool> UpdateItemAsync(BorrowingItem item);
}