using BooksCatalogService.Models;

namespace BooksCatalogService.Repository
{
    public interface IBookRepository
    {
        Task<List<Book>> GetAllAsync(string? title, string? author);
        Task<Book?> GetByIdAsync(int id);
        Task<Book> AddAsync(Book book);
        Task<Book> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
        Task<bool> ReserveBookAsync(int bookId); // Trừ 1
        Task<bool> ReturnBookAsync(int bookId);  // Cộng 1
        Task<List<Category>> GetCategoriesByIdsAsync(List<int> ids);
    }
}

