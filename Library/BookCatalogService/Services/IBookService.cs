using BooksCatalogService.Dtos;
using BooksCatalogService.Models;

namespace BooksCatalogService.Services
{
    public interface IBookService
    {
        Task<ServiceResponse<List<BookDto>>> GetAllBooks(string? title, string? author);
    
        Task<ServiceResponse<BookDto>> GetBookById(int id);
    
        Task<ServiceResponse<BookDto>> AddBook(CreateBookDto newBook);
    
        Task<ServiceResponse<BookDto>> UpdateBook(int id, UpdateBookDto updatedBook);
    
        Task<ServiceResponse<bool>> DeleteBook(int id);
    }
}
