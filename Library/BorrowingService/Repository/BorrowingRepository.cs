using BorrowingService.Data;
using BorrowingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BorrowingService.Repository;

public class BorrowingRepository : IBorrowingRepository
{
    private readonly BorrowingDbContext _context;

    public BorrowingRepository(BorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Borrowing> AddBorrowingAsync(Borrowing borrowing)
    {
        // EF Core sẽ tự động track và insert cả Borrowing lẫn List<BorrowingItem>
        await _context.Borrowings.AddAsync(borrowing);
        await _context.SaveChangesAsync();
        return borrowing;
    }

    public async Task<BorrowingItem?> GetItemByIdAsync(int itemId)
    {
        // Lấy item và Include luôn thông tin phiếu mượn cha (nếu cần kiểm tra User)
        return await _context.BorrowingItems
            .Include(i => i.Borrowing)
            .FirstOrDefaultAsync(i => i.Id == itemId);
    }

    public async Task<bool> UpdateItemAsync(BorrowingItem item)
    {
        _context.BorrowingItems.Update(item);
        return await _context.SaveChangesAsync() > 0;
    }
}