using BorrowingService.Dtos;
using BorrowingService.Models;

namespace BorrowingService.Service;

public interface IBorrowingService
{
    // Mượn sách (UserId lấy từ Token nên truyền vào đây)
    Task<ServiceResponse<BorrowingResponseDto>> BorrowBookAsync(int userId, BorrowingRequestDto request);

    // Trả sách (Trả từng cuốn item)
    Task<ServiceResponse<bool>> ReturnBookAsync(int userId, int borrowingItemId);
}