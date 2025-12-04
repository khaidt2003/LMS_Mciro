using BooksCatalogService.Dtos;
using BooksCatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksCatalogService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? title, [FromQuery] string? author)
        {
            return Ok(await _bookService.GetAllBooks(title, author));
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingle(int id)
        {
            var response = await _bookService.GetBookById(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]  
        public async Task<IActionResult> AddBook(CreateBookDto newBook)
        {
            return Ok(await _bookService.AddBook(newBook));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updatedBook)
        {
            var response = await _bookService.UpdateBook(id, updatedBook);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteBook(int id)
        {
            var response = await _bookService.DeleteBook(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}

