using System.ComponentModel.DataAnnotations;

namespace BooksCatalogService.Dtos;

public class CreateBookDto
{
    [Required]
    [StringLength(255)]
    public required string Title { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string AuthorName { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be a negative number.")]
    public required int AvailableCopies { get; set; }
    
    [Required(ErrorMessage = "Book must belong to at least one category.")]
    [MinLength(1, ErrorMessage = "Please select at least one category.")]
    public required List<int> CategoryIds { get; set; }
}