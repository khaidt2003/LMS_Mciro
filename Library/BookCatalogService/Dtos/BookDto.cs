namespace BooksCatalogService.Dtos;

public class BookDto
{
    public int?  Id { get; set; }
    public string  Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int  AvailableCopies { get; set; }
    
    public List<string> CategoryNames { get; set; } = new List<string>();
}