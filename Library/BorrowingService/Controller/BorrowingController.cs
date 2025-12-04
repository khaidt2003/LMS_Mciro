using System.Security.Claims;
using BorrowingService.Dtos;
using BorrowingService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BorrowingService.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]  
public class BorrowingController : ControllerBase
{
    private readonly IBorrowingService _borrowingService;

    public BorrowingController(IBorrowingService borrowingService)
    {
        _borrowingService = borrowingService;
    }
    // POST: api/borrowing/borrow
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook(BorrowingRequestDto request)
    {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Token không hợp lệ (Thiếu UserId).");
            }

            int userId = int.Parse(userIdClaim);
            
            var response = await _borrowingService.BorrowBookAsync(userId, request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // POST: api/borrowing/return
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] int itemId)
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int userId = int.Parse(userIdClaim);
            
            var response = await _borrowingService.ReturnBookAsync(userId, itemId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
}