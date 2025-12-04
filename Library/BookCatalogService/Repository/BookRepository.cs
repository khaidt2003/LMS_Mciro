using BooksCatalogService.Data;
using BooksCatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksCatalogService.Repository;

public class BookRepository : IBookRepository
{
    private readonly BookDbContext _context;

    public BookRepository(BookDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync(string? title, string? author)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(b => b.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.Contains(author));
        }

        return await query.ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book> AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        return book;
    }

    public async Task<Book> UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        return book;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        return true;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<List<Category>> GetCategoriesByIdsAsync(List<int> ids)
    {
        return await _context.Categories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync();
    }
    public async Task<bool> ReserveBookAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
    
        // Nếu sách không tồn tại hoặc số lượng <= 0 thì thất bại
        if (book == null || (book.AvailableCopies ?? 0) <= 0) 
        {
            return false;
        }

        book.AvailableCopies = (book.AvailableCopies ?? 0) - 1;
    
        // Đánh dấu đã sửa và lưu
        _context.Books.Update(book);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ReturnBookAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
    
        if (book == null) 
        {
            return false;
        }

        // Cộng thêm 1 vào kho
        book.AvailableCopies = (book.AvailableCopies ?? 0) + 1;
    
        _context.Books.Update(book);
        return await _context.SaveChangesAsync() > 0;
    }
}