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

        public async Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync(string? filterOn = null, string? filterQuery = null,string? sortBy = null, bool isAscending = true,int pageNumber = 1, int pageSize = 1000)
        {
            var query = _dbContext.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                switch (filterOn.Trim().ToLowerInvariant())
                {
                    case "title":
                        query = query.Where(b => b.Title.Contains(filterQuery)); break;
                    case "description":
                        query = query.Where(b => b.Description.Contains(filterQuery)); break;
                    case "genre":
                        query = query.Where(b => b.Genre.Contains(filterQuery)); break;
                    case "rate":
                        query = int.TryParse(filterQuery, out var rate)
                            ? query.Where(b => b.IsRead && b.Rate == rate)
                            : query.Where(b => false);   
                        break;
                }
            }

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "title" => isAscending ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
                "genre" => isAscending ? query.OrderBy(b => b.Genre) : query.OrderByDescending(b => b.Genre),
                "rate" => isAscending ? query.OrderBy(b => b.Rate) : query.OrderByDescending(b => b.Rate),
                "dateadded" => isAscending ? query.OrderBy(b => b.DateAdded) : query.OrderByDescending(b => b.DateAdded),
                _ => query.OrderBy(b => b.Id)
            };

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 1000);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(book => new BookWithAuthorAndPublisherDTO
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
                    PublisherID = book.PublisherID,
                    AuthorIds = book.Book_Authors.Select(n => n.AuthorId).ToList(),
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                })
                .ToListAsync();
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
                    PublisherID = book.PublisherID,
                    AuthorIds = book.Book_Authors.Select(n => n.AuthorId).ToList(),
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

            var existingIds = bookDomain.Book_Authors.Select(x => x.AuthorId).ToList();
            var toRemove = bookDomain.Book_Authors.Where(x => !authorIds.Contains(x.AuthorId)).ToList();
            var toAdd = authorIds.Except(existingIds)
                .Select(aid => new Book_Author { BookId = id, AuthorId = aid });

            _dbContext.Books_Authors.RemoveRange(toRemove);
            await _dbContext.Books_Authors.AddRangeAsync(toAdd);
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

        public async Task<bool> PublisherExistsAsync(int publisherId)
        {
            return await _dbContext.Publishers.AnyAsync(p => p.Id == publisherId);
        }

        public async Task<List<int>> GetMissingAuthorIdsAsync(List<int> authorIds)
        {
            var existing = await _dbContext.Authors
                .Where(a => authorIds.Contains(a.Id))
                .Select(a => a.Id).ToListAsync();
            return authorIds.Except(existing).ToList();
        }

        public async Task<bool> TitleExistsInPublisherAsync(string title, int publisherId, int? excludeBookId = null)
        {
            return await _dbContext.Books.AnyAsync(b =>
                b.PublisherID == publisherId && b.Title == title &&
                (excludeBookId == null || b.Id != excludeBookId));
        }

        public async Task<List<int>> GetAuthorIdsOverLimitAsync(List<int> authorIds, int maxBooks, int? excludeBookId = null)
        {
            return await _dbContext.Books_Authors
                .Where(ba => authorIds.Contains(ba.AuthorId) &&
                             (excludeBookId == null || ba.BookId != excludeBookId))
                .GroupBy(ba => ba.AuthorId)
                .Where(g => g.Count() >= maxBooks)
                .Select(g => g.Key)
                .ToListAsync();
        }

        public async Task<int> CountBooksByPublisherInYearAsync(int publisherId, int year, int? excludeBookId = null)
        {
            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);
            return await _dbContext.Books.CountAsync(b =>
                b.PublisherID == publisherId &&
                b.DateAdded >= start && b.DateAdded < end &&
                (excludeBookId == null || b.Id != excludeBookId));
        }
    }
}