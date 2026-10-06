using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/book-authors")]
    [ApiController]
    public class BookAuthorsController : ControllerBase
    {
        private readonly IBookAuthorRepository _repository;

        public BookAuthorsController(IBookAuthorRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        [Authorize(Roles = "Write")]
        public async Task<IActionResult> AddBookAuthor([FromBody] AddBookAuthorRequestDTO dto)
        {
            if (!await _repository.BookExistsAsync(dto.BookId))
                return BadRequest(new { message = $"Sách có Id = {dto.BookId} không tồn tại" });

            if (!await _repository.AuthorExistsAsync(dto.AuthorId))
                return BadRequest(new { message = $"Tác giả có Id = {dto.AuthorId} không tồn tại" });

            if (await _repository.RelationExistsAsync(dto.BookId, dto.AuthorId))
                return Conflict(new { message = "Tác giả đã được gán cho sách này" });

            await _repository.AddAsync(dto.BookId, dto.AuthorId);
            return Ok(dto);
        }
    }
}