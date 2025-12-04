using BooksCatalogService.Dtos;
using BooksCatalogService.Models;
using BooksCatalogService.Repository;

namespace BooksCatalogService.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<ServiceResponse<List<BookDto>>> GetAllBooks(string? title, string? author)
    {
        var response = new ServiceResponse<List<BookDto>>();
        var books = await _bookRepository.GetAllAsync(title, author);
        response.Data = books.Select(book => new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            AvailableCopies =  (int)book.AvailableCopies!,
            CategoryNames = book.BookCategories.Select(bc => bc.Category.Name).ToList()
        }).ToList();
        response.Message = "Books retrieved successfully.";
        return response;
    }

    public async Task<ServiceResponse<BookDto>> GetBookById(int id)
    {
        
        var response = new ServiceResponse<BookDto>();
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            response.Success = false;
            response.Message = "Book not found.";
            return response;
        }
        response.Data = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            AvailableCopies = (int)book.AvailableCopies!,
            CategoryNames = book.BookCategories.Select(bc => bc.Category.Name).ToList()
        };
        return response;
    }

    public async Task<ServiceResponse<BookDto>> AddBook(CreateBookDto newBook)
    {
        
        var response = new ServiceResponse<BookDto>();
        
        var categories = await _bookRepository.GetCategoriesByIdsAsync(newBook.CategoryIds);
        
        if (categories.Count != newBook.CategoryIds.Count)
        {
            response.Success = false;
            response.Message = "One or more categories do not exist.";
            return response;
        }
        var book = new Book
        {
            Title = newBook.Title,
            Author = newBook.AuthorName,
            AvailableCopies = newBook.AvailableCopies
        };
        foreach (var category in categories)
        {
            book.BookCategories.Add(new BookCategory
            {
                Book = book,
                Category = category
            });
        }
        
        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();

        response.Data = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            AvailableCopies = (int)book.AvailableCopies!,
            CategoryNames = categories.Select(c => c.Name).ToList()
        };
        
        response.Message = "Book created successfully.";
        return response;
    }

    public async Task<ServiceResponse<BookDto>> UpdateBook(int id, UpdateBookDto updatedBook)
    {
        var response = new ServiceResponse<BookDto>();
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            response.Success = false;
            response.Message = "Book not found.";
            return response;
        }

        if (!string.IsNullOrEmpty(updatedBook.Title))
        {
            book.Title = updatedBook.Title;
        }
        
        if (!string.IsNullOrEmpty(updatedBook.Author))
        {
            book.Author = updatedBook.Author;
        }
        Console.WriteLine(updatedBook.AvailableCopies);
        if (updatedBook.AvailableCopies.HasValue)
        {
            book.AvailableCopies = updatedBook.AvailableCopies.Value;
        }


    
        await _bookRepository.UpdateAsync(book);
        await _bookRepository.SaveChangesAsync();
        
        response.Data = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            AvailableCopies = (int)book.AvailableCopies!,
            CategoryNames = book.BookCategories.Select(bc => bc.Category.Name).ToList()
        };
    
        response.Message = "Book updated successfully.";
        return response;
    }

    public async Task<ServiceResponse<bool>> DeleteBook(int id)
    {
        var response = new ServiceResponse<bool>();
        var success = await _bookRepository.DeleteAsync(id);
        if (!success)
        {
            response.Success = false;
            response.Message = "Book not found.";
            return response;
        }
        
        await _bookRepository.SaveChangesAsync();
        response.Data = true;
        response.Message = "Book deleted successfully.";
        return response;
    }
}
