using System.Security.Claims;
using BorrowingService.Dtos;
using BorrowingService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BorrowingService.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Bắt buộc phải có Token mới gọi được
public class BorrowingController : ControllerBase
{
    private readonly IBorrowingService _borrowingService;

    public BorrowingController(IBorrowingService borrowingService)
    {
        _borrowingService = borrowingService;
    }
        // POST: api/borrowing/borrow
        // Use Case 9: Mượn sách
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook(BorrowingRequestDto request)
    {
            // 1. Lấy UserId từ Token (User không cần nhập, tránh giả mạo)
            // Lưu ý: Claim tên là "userId" hoặc "nameid" tùy vào cách bạn config bên UserService
            // Ở đây tôi giả định claim tên là "userId"
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Token không hợp lệ (Thiếu UserId).");
            }

            int userId = int.Parse(userIdClaim);

            // 2. Gọi Service xử lý
            var response = await _borrowingService.BorrowBookAsync(userId, request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // POST: api/borrowing/return
        // Use Case 10: Trả sách
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] int itemId)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            // Gọi Service trả sách
            var response = await _borrowingService.ReturnBookAsync(userId, itemId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
}