using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("get-all-books")]
        public async Task<IActionResult> GetAll()
        {
            var allBooks = await _bookRepository.GetAllBooksAsync();
            return Ok(allBooks);
        }

        [HttpGet("get-book-by-id/{id:int}")]
        public async Task<IActionResult> GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = await _bookRepository.GetBookByIdAsync(id);
            if (bookWithIdDTO == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }
            return Ok(bookWithIdDTO);
        }

        [HttpPost("add-book")]
        public async Task<IActionResult> AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            var bookAdd = await _bookRepository.AddBookAsync(addBookRequestDTO);
            if (bookAdd == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB hoặc tác giả" });
            }
            return Ok(bookAdd);
        }

        [HttpPut("update-book-by-id/{id:int}")]
        public async Task<IActionResult> UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            var updateBook = await _bookRepository.UpdateBookByIdAsync(id, bookDTO);
            if (updateBook == null)
            {
                return NotFound(new { message = "Không tìm thấy sách, NXB hoặc tác giả" });
            }
            return Ok(updateBook);
        }

        [HttpDelete("delete-book-by-id/{id:int}")]
        public async Task<IActionResult> DeleteBookById(int id)
        {
            var deleteBook = await _bookRepository.DeleteBookByIdAsync(id);
            if (deleteBook == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }
            return Ok(new { message = "Đã xóa sách" });
        }
    }
}