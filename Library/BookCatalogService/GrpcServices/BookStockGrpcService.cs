using BooksCatalogService.Protos;
using BooksCatalogService.Repository;
using Grpc.Core;

namespace BooksCatalogService.GrpcServices;

public class BookStockGrpcService : BookStockService.BookStockServiceBase
{
    private readonly IBookRepository _repository;
    private readonly ILogger<BookStockGrpcService> _logger;

    public BookStockGrpcService(IBookRepository repository, ILogger<BookStockGrpcService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<CheckAvailabilityResponse> CheckAvailability(CheckAvailabilityRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC CheckAvailability called for Book ID: {BookId}", request.BookId);
        var response = new CheckAvailabilityResponse { Exists = false, IsAvailable = false, AvailableCopies = 0 };
        
        var book = await _repository.GetByIdAsync(request.BookId);

        if (book != null)
        {
            response.Exists = true;
            response.AvailableCopies = (int)book.AvailableCopies!;
            response.IsAvailable = book.AvailableCopies > 0;
        }

        return response;
    }

    public override async Task<BookActionResponse> ReserveBook(BookActionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ReserveBook called for Book ID: {BookId}", request.BookId);
        var response = new BookActionResponse { Success = false };

        var book = await _repository.GetByIdAsync(request.BookId);

        if (book != null && book.AvailableCopies > 0)
        {
            book.AvailableCopies--;
            await _repository.UpdateAsync(book);
            await _repository.SaveChangesAsync();
            response.Success = true;
        }

        return response;
    }

    public override async Task<BookActionResponse> ReturnBook(BookActionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ReturnBook called for Book ID: {BookId}", request.BookId);
        var response = new BookActionResponse { Success = false };

        var book = await _repository.GetByIdAsync(request.BookId);

        if (book != null)
        {
            book.AvailableCopies++;
            await _repository.UpdateAsync(book);
            await _repository.SaveChangesAsync();
            response.Success = true;
        }

        return response;
    }
}