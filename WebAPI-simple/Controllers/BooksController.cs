using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.CustomActionFilters;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private const int MaxBooksPerAuthor = 20;              
        private const int MaxBooksPerPublisherPerYear = 100;   

        private readonly IBookRepository _bookRepository;

        public BooksController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        [HttpGet("get-all-books")]
        public async Task<IActionResult> GetAll(
        [FromQuery] string? filterOn, [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy, [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var allBooks = await _bookRepository.GetAllBooksAsync(
                filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
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
        [ValidateModel]
        public async Task<IActionResult> AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            if (!await ValidateBookAsync(addBookRequestDTO))
            {
                return BadRequest(ModelState);
            }

            var bookAdd = await _bookRepository.AddBookAsync(addBookRequestDTO);
            if (bookAdd == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB hoặc tác giả" });
            }
            return Ok(bookAdd);
        }

        [HttpPut("update-book-by-id/{id:int}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            if (!await ValidateBookAsync(bookDTO, id))
            {
                return BadRequest(ModelState);
            }

            var updateBook = await _bookRepository.UpdateBookByIdAsync(id, bookDTO);
            if (updateBook == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
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

        #region Private methods
        private async Task<bool> ValidateBookAsync(AddBookRequestDTO dto, int? excludeBookId = null)
        {
            if (dto == null)
            {
                ModelState.AddModelError(nameof(dto), "Please add book data");
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                ModelState.AddModelError(nameof(dto.Description), $"{nameof(dto.Description)} cannot be null");
            }

            if (!await _bookRepository.PublisherExistsAsync(dto.PublisherID))
            {
                ModelState.AddModelError(nameof(dto.PublisherID),
                    $"Nhà xuất bản có Id = {dto.PublisherID} không tồn tại");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.Title) &&
                    await _bookRepository.TitleExistsInPublisherAsync(dto.Title.Trim(), dto.PublisherID, excludeBookId))
                {
                    ModelState.AddModelError(nameof(dto.Title), "NXB này đã có sách trùng tên");
                }

                var year = dto.DateAdded.Year;
                var count = await _bookRepository.CountBooksByPublisherInYearAsync(dto.PublisherID, year, excludeBookId);
                if (count >= MaxBooksPerPublisherPerYear)
                {
                    ModelState.AddModelError(nameof(dto.PublisherID),
                        $"NXB đã xuất bản tối đa {MaxBooksPerPublisherPerYear} sách trong năm {year}");
                }
            }

            var authorIds = dto.AuthorIds.Distinct().ToList();
            var missing = await _bookRepository.GetMissingAuthorIdsAsync(authorIds);
            if (missing.Count > 0)
            {
                ModelState.AddModelError(nameof(dto.AuthorIds),
                    $"Không tìm thấy tác giả có Id: {string.Join(", ", missing)}");
            }
            else
            {
                var over = await _bookRepository.GetAuthorIdsOverLimitAsync(authorIds, MaxBooksPerAuthor, excludeBookId);
                if (over.Count > 0)
                {
                    ModelState.AddModelError(nameof(dto.AuthorIds),
                        $"Tác giả Id {string.Join(", ", over)} đã đạt tối đa {MaxBooksPerAuthor} cuốn");
                }
            }

            return ModelState.ErrorCount == 0;
        }
        #endregion
    }
}