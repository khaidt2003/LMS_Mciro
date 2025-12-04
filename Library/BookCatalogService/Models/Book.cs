namespace BooksCatalogService.Models;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public int? AvailableCopies { get; set; }

    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
