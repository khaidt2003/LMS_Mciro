using BorrowingService.Dtos;
using BorrowingService.GrpcService;
using BorrowingService.Models;
using BorrowingService.QueueMessageService;
using BorrowingService.Repository;

namespace BorrowingService.Service;

public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _repository;
        private readonly IBookGrpcClient _grpcClient; // Để gọi kho sách
        private readonly IMessageProducer _messageProducer; // Để gửi tin nhắn

        public BorrowingService(
            IBorrowingRepository repository,
            IBookGrpcClient grpcClient,
            IMessageProducer messageProducer)
        {
            _repository = repository;
            _grpcClient = grpcClient;
            _messageProducer = messageProducer;
        }

        public async Task<ServiceResponse<BorrowingResponseDto>> BorrowBookAsync(int userId, BorrowingRequestDto request)
        {
            var response = new ServiceResponse<BorrowingResponseDto>();

            // 1. CHECK STOCK (Gọi gRPC)
            // Kiểm tra xem tất cả sách có tồn tại và còn hàng không TRƯỚC khi lưu
            foreach (var item in request.BorrowItems)
            {
                var stockCheck = _grpcClient.CheckAvailability(item.BookId);
                Console.WriteLine($"Checking availability for Bo1ok ID {item.BookId}: {stockCheck.IsAvailable}");
                if (!stockCheck.IsAvailable)
                {
                    response.Success = false;
                    response.Message = $"Sách có ID {item.BookId} không tồn tại hoặc đã hết hàng.";
                    return response;
                }
            }

            // 2. LƯU DATABASE (Tạo phiếu mượn)
            var borrowing = new Borrowing
            {
                UserId = userId,
                DueDate = request.DueDate,
                BorrowDate = DateTime.UtcNow,
                Status = "Borrowed"
            };

            foreach (var itemDto in request.BorrowItems)
            {
                borrowing.BorrowingItems.Add(new BorrowingItem
                {
                    BookId = itemDto.BookId,
                    Quantity = itemDto.Quantity,
                    Status = "Borrowed"
                });
            }

            await _repository.AddBorrowingAsync(borrowing);

            // 3. TRỪ TỒN KHO & GỬI MESSAGE (Xử lý hậu kỳ)
            foreach (var item in borrowing.BorrowingItems)
            {
                // Gọi gRPC để trừ số lượng trong kho
                _grpcClient.ReserveBook(item.BookId);
                
                // Gửi 2 tin nhắn RabbitMQ: 1 cho history, 1 cho notification
                var historyMsg = new { Event = "BorrowHistoryUpdated", BorrowingId = borrowing.Id, UserId = userId, BookId = item.BookId, BorrowDate = borrowing.BorrowDate };
                await _messageProducer.SendMessageAsync(historyMsg, "history.borrow");

                var notificationMsg = new { Event = "BookBorrowed", UserId = userId, BookId = item.BookId, Title = _grpcClient.CheckAvailability(item.BookId).Title, DueDate = borrowing.DueDate };
                await _messageProducer.SendMessageAsync(notificationMsg, "email.borrow");
            }

            // 4. Trả về kết quả
            response.Data = new BorrowingResponseDto
            {
                Id = borrowing.Id,
                UserId = borrowing.UserId,
                Status = borrowing.Status,
                Items = request.BorrowItems
            };
            response.Message = "Mượn sách thành công!";

            return response;
        }

        public async Task<ServiceResponse<bool>> ReturnBookAsync(int userId, int borrowingItemId)
        {
            var response = new ServiceResponse<bool>();

            // 1. Lấy thông tin phiếu mượn chi tiết
            var item = await _repository.GetItemByIdAsync(borrowingItemId);

            if (item == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy thông tin sách mượn.";
                return response;
            }

            // Kiểm tra xem có đúng người mượn trả không (nếu cần)
            if (item.Borrowing.UserId != userId)
            {
                response.Success = false;
                response.Message = "Bạn không có quyền trả cuốn sách này.";
                return response;
            }

            if (item.Status == "Returned")
            {
                response.Success = false;
                response.Message = "Sách này đã được trả trước đó.";
                return response;
            }

            // 2. CẬP NHẬT DB
            item.Status = "Returned";
            item.ReturnDate = DateTime.UtcNow;
            await _repository.UpdateItemAsync(item);

            // 3. CỘNG TỒN KHO (Gọi gRPC)
            _grpcClient.ReturnBook(item.BookId);

            // 4. GỬI 2 TIN NHẮN RABBITMQ
            var historyMsg = new { Event = "ReturnHistoryUpdated", BorrowingItemId = item.Id, ReturnDate = item.ReturnDate };
            await _messageProducer.SendMessageAsync(historyMsg, "history.return");

            var notificationMsg = new { Event = "BookReturned", UserId = userId, BookId = item.BookId, Title = _grpcClient.CheckAvailability(item.BookId).Title, ReturnDate = item.ReturnDate };
            await _messageProducer.SendMessageAsync(notificationMsg, "email.return");

            response.Data = true;
            response.Message = "Trả sách thành công.";
            return response;
        }
}