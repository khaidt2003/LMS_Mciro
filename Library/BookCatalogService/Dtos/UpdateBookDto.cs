using System.ComponentModel.DataAnnotations;

namespace BooksCatalogService.Dtos;

public class UpdateBookDto
{
    [StringLength(255)]
    public string? Title  { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Author { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be a negative number.")]
    public int? AvailableCopies { get; set; }
}