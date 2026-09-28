using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorsController(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        [HttpGet("get-all-author")]
        public async Task<IActionResult> GetAllAuthor()
        {
            var allAuthors = await _authorRepository.GetAllAuthorsAsync();
            return Ok(allAuthors);
        }

        [HttpGet("get-author-by-id/{id:int}")]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var authorWithId = await _authorRepository.GetAuthorByIdAsync(id);
            if (authorWithId == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả" });
            }
            return Ok(authorWithId);
        }

        [HttpPost("add-author")]
        public async Task<IActionResult> AddAuthor([FromBody] AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorAdd = await _authorRepository.AddAuthorAsync(addAuthorRequestDTO);
            return Ok(authorAdd);
        }

        [HttpPut("update-author-by-id/{id:int}")]
        public async Task<IActionResult> UpdateAuthorById(int id, [FromBody] AuthorNoIdDTO authorDTO)
        {
            var authorUpdate = await _authorRepository.UpdateAuthorByIdAsync(id, authorDTO);
            if (authorUpdate == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả" });
            }
            return Ok(authorUpdate);
        }

        [HttpDelete("delete-author-by-id/{id:int}")]
        public async Task<IActionResult> DeleteAuthorById(int id)
        {
            var authorDelete = await _authorRepository.DeleteAuthorByIdAsync(id);
            if (authorDelete == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả" });
            }
            return Ok(new { message = "Đã xóa tác giả" });
        }

        [HttpGet("{id:int}/books")]
        public async Task<IActionResult> GetBooksByAuthorId(int id)
        {
            var result = await _authorRepository.GetBooksByAuthorIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy tác giả" });
            }
            return Ok(result);
        }
    }
}