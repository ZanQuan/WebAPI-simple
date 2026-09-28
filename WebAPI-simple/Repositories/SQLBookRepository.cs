using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;
        public SQLBookRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync()
        {
            var allBooks = await _dbContext.Books.Select(book => new BookWithAuthorAndPublisherDTO()
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
            }).ToListAsync();

            return allBooks;
        }

        public async Task<BookWithAuthorAndPublisherDTO?> GetBookByIdAsync(int id)
        {
            var bookDTO = await _dbContext.Books
                .Where(n => n.Id == id)
                .Select(book => new BookWithAuthorAndPublisherDTO()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.DateRead,
                    Rate = book.Rate,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    DateAdded = book.DateAdded,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                }).FirstOrDefaultAsync();

            return bookDTO;  
        }

        public async Task<AddBookRequestDTO?> AddBookAsync(AddBookRequestDTO addBookRequestDTO)
        {
            var publisherExists = await _dbContext.Publishers
                .AnyAsync(p => p.Id == addBookRequestDTO.PublisherID);
            if (!publisherExists) return null;

            var authorIds = (addBookRequestDTO.AuthorIds ?? new List<int>()).Distinct().ToList();
            var authorCount = await _dbContext.Authors.CountAsync(a => authorIds.Contains(a.Id));
            if (authorCount != authorIds.Count) return null;

            var bookDomainModel = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherID = addBookRequestDTO.PublisherID,
                Book_Authors = authorIds.Select(aid => new Book_Author { AuthorId = aid }).ToList()
            };

            _dbContext.Books.Add(bookDomainModel);
            await _dbContext.SaveChangesAsync();

            return addBookRequestDTO;
        }

        public async Task<AddBookRequestDTO?> UpdateBookByIdAsync(int id, AddBookRequestDTO bookDTO)
        {
            var bookDomain = await _dbContext.Books
                .Include(b => b.Book_Authors)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (bookDomain == null) return null;

            var publisherExists = await _dbContext.Publishers
                .AnyAsync(p => p.Id == bookDTO.PublisherID);
            if (!publisherExists) return null;

            var authorIds = (bookDTO.AuthorIds ?? new List<int>()).Distinct().ToList();
            var authorCount = await _dbContext.Authors.CountAsync(a => authorIds.Contains(a.Id));
            if (authorCount != authorIds.Count) return null;

            bookDomain.Title = bookDTO.Title;
            bookDomain.Description = bookDTO.Description;
            bookDomain.IsRead = bookDTO.IsRead;
            bookDomain.DateRead = bookDTO.DateRead;
            bookDomain.Rate = bookDTO.Rate;
            bookDomain.Genre = bookDTO.Genre;
            bookDomain.CoverUrl = bookDTO.CoverUrl;
            bookDomain.DateAdded = bookDTO.DateAdded;
            bookDomain.PublisherID = bookDTO.PublisherID;

            _dbContext.Books_Authors.RemoveRange(bookDomain.Book_Authors);
            await _dbContext.Books_Authors.AddRangeAsync(
                authorIds.Select(aid => new Book_Author { BookId = id, AuthorId = aid }));

            await _dbContext.SaveChangesAsync();
            return bookDTO;
        }

        public async Task<Book?> DeleteBookByIdAsync(int id)
        {
            var bookDomain = await _dbContext.Books.FirstOrDefaultAsync(n => n.Id == id);
            if (bookDomain == null) return null;

            var links = await _dbContext.Books_Authors.Where(x => x.BookId == id).ToListAsync();
            _dbContext.Books_Authors.RemoveRange(links);
            _dbContext.Books.Remove(bookDomain);

            await _dbContext.SaveChangesAsync();
            return bookDomain;
        }
    }
}