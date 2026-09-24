using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public BooksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            var allBooksDTO = _dbContext.Books.Select(book => new BookWithAuthorAndPublisherDTO()
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead : null,
                Rate = book.IsRead ? book.Rate : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherName = book.Publisher.Name,
                AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
            }).ToList();

            return Ok(allBooksDTO);
        }

        [HttpGet]
        [Route("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors).ThenInclude(ba => ba.Author)
                .FirstOrDefault(b => b.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            var bookDTO = new BookWithAuthorAndPublisherDTO()
            {
                Id = bookDomain.Id,
                Title = bookDomain.Title,
                Description = bookDomain.Description,
                IsRead = bookDomain.IsRead,
                DateRead = bookDomain.DateRead,
                Rate = bookDomain.Rate,
                Genre = bookDomain.Genre,
                CoverUrl = bookDomain.CoverUrl,
                DateAdded = bookDomain.DateAdded,
                PublisherName = bookDomain.Publisher != null ? bookDomain.Publisher.Name : "Unknown",
                AuthorNames = bookDomain.Book_Authors?
                    .Where(y => y.Author != null)
                    .Select(y => y.Author.FullName).ToList() ?? new List<string>()
            };
            return Ok(bookDTO);
        }

        [HttpPost("add-book")]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            var publisherDomain = _dbContext.Publishers
                .FirstOrDefault(x => x.Id == addBookRequestDTO.PublisherID);
            if (publisherDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }

            var authorIds = addBookRequestDTO.AuthorIds ?? new List<int>();
            foreach (var authorId in authorIds)
            {
                if (!_dbContext.Authors.Any(x => x.Id == authorId))
                {
                    return NotFound(new { message = "Không tìm thấy tác giả có Id = " + authorId });
                }
            }
            var bookDomain = new Book()
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = publisherDomain.Id
            };
            _dbContext.Books.Add(bookDomain);
            _dbContext.SaveChanges();   
            foreach (var authorId in authorIds)
            {
                _dbContext.Books_Authors.Add(new Book_Author()
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorId
                });
            }
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPut("update-book-by-id/{id:int}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(x => x.Id == id);
            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            if (!_dbContext.Publishers.Any(x => x.Id == addBookRequestDTO.PublisherID))
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }

            var authorIds = addBookRequestDTO.AuthorIds ?? new List<int>();
            foreach (var authorId in authorIds)
            {
                if (!_dbContext.Authors.Any(x => x.Id == authorId))
                {
                    return NotFound(new { message = "Không tìm thấy tác giả có Id = " + authorId });
                }
            }

            bookDomain.Title = addBookRequestDTO.Title;
            bookDomain.Description = addBookRequestDTO.Description;
            bookDomain.IsRead = addBookRequestDTO.IsRead;
            bookDomain.DateRead = addBookRequestDTO.DateRead;
            bookDomain.Rate = addBookRequestDTO.Rate;
            bookDomain.Genre = addBookRequestDTO.Genre;
            bookDomain.CoverUrl = addBookRequestDTO.CoverUrl;
            bookDomain.DateAdded = addBookRequestDTO.DateAdded;
            bookDomain.PublisherID = addBookRequestDTO.PublisherID;
            _dbContext.SaveChanges();

            var existingBookAuthors = _dbContext.Books_Authors.Where(x => x.BookId == id).ToList();
            if (existingBookAuthors.Count > 0)
            {
                _dbContext.Books_Authors.RemoveRange(existingBookAuthors);
                _dbContext.SaveChanges();
            }
            foreach (var authorId in authorIds)
            {
                _dbContext.Books_Authors.Add(new Book_Author()
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorId
                });
            }
            _dbContext.SaveChanges();

            return Ok(addBookRequestDTO);
        }

        [HttpDelete("delete-book-by-id/{id:int}")]
        public IActionResult DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(x => x.Id == id);
            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }
            var existingBookAuthors = _dbContext.Books_Authors.Where(x => x.BookId == id).ToList();
            if (existingBookAuthors.Count > 0)
            {
                _dbContext.Books_Authors.RemoveRange(existingBookAuthors);
                _dbContext.SaveChanges();
            }

            _dbContext.Books.Remove(bookDomain);
            _dbContext.SaveChanges();
            return Ok(new { message = "Đã xóa sách" });
        }
    }
}