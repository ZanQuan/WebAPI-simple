using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public class SQLAuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLAuthorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AuthorDTO>> GetAllAuthorsAsync(
    string? filterOn = null, string? filterQuery = null,
    string? sortBy = null, bool isAscending = true,
    int pageNumber = 1, int pageSize = 1000)
        {
            var query = _dbContext.Authors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                var field = filterOn.Trim().ToLowerInvariant();
                if (field == "fullname" || field == "name")
                {
                    query = query.Where(a => a.FullName.Contains(filterQuery));
                }
            }

            var sortField = sortBy?.Trim().ToLowerInvariant();
            if (sortField == "fullname" || sortField == "name")
            {
                query = isAscending
                    ? query.OrderBy(a => a.FullName)
                    : query.OrderByDescending(a => a.FullName);
            }
            else
            {
                query = query.OrderBy(a => a.Id);
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 1000);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuthorDTO { Id = a.Id, FullName = a.FullName })
                .ToListAsync();
        }

        public async Task<AuthorNoIdDTO?> GetAuthorByIdAsync(int id)
        {
            return await _dbContext.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorNoIdDTO { FullName = a.FullName })
                .FirstOrDefaultAsync();
        }

        public async Task<AddAuthorRequestDTO> AddAuthorAsync(AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorDomainModel = new Author
            {
                FullName = addAuthorRequestDTO.FullName
            };
            _dbContext.Authors.Add(authorDomainModel);
            await _dbContext.SaveChangesAsync();
            return addAuthorRequestDTO;
        }

        public async Task<AuthorNoIdDTO?> UpdateAuthorByIdAsync(int id, AuthorNoIdDTO authorNoIdDTO)
        {
            var authorDomain = await _dbContext.Authors.FirstOrDefaultAsync(n => n.Id == id);
            if (authorDomain == null) return null;

            authorDomain.FullName = authorNoIdDTO.FullName;
            await _dbContext.SaveChangesAsync();
            return authorNoIdDTO;
        }

        public async Task<Author?> DeleteAuthorByIdAsync(int id)
        {
            var authorDomain = await _dbContext.Authors.FirstOrDefaultAsync(n => n.Id == id);
            if (authorDomain == null) return null;

            var links = await _dbContext.Books_Authors.Where(x => x.AuthorId == id).ToListAsync();
            _dbContext.Books_Authors.RemoveRange(links);
            _dbContext.Authors.Remove(authorDomain);

            await _dbContext.SaveChangesAsync();
            return authorDomain;
        }

        public async Task<AuthorWithBooksDTO?> GetBooksByAuthorIdAsync(int id)
        {
            return await _dbContext.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorWithBooksDTO
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    BookTitles = a.Book_Authors.Select(ba => ba.Book.Title).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> HasBooksAsync(int id)
        {
            return await _dbContext.Books_Authors.AnyAsync(ba => ba.AuthorId == id);
        }
    }
}