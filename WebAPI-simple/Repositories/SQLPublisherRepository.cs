using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLPublisherRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PublisherDTO>> GetAllPublishersAsync(
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 1000)
        {
            var query = _dbContext.Publishers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery)
                && filterOn.Trim().Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Name.Contains(filterQuery));
            }

            if (!string.IsNullOrWhiteSpace(sortBy) && sortBy.Trim().Equals("name", StringComparison.OrdinalIgnoreCase))
                query = isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
            else
                query = query.OrderBy(p => p.Id);

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 1000);

            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new PublisherDTO { Id = p.Id, Name = p.Name })
                .ToListAsync();
        }

        public async Task<PublisherNoIdDTO?> GetPublisherByIdAsync(int id)
        {
            return await _dbContext.Publishers
                .Where(p => p.Id == id)
                .Select(p => new PublisherNoIdDTO { Name = p.Name })
                .FirstOrDefaultAsync();
        }

        public async Task<AddPublisherRequestDTO> AddPublisherAsync(AddPublisherRequestDTO addPublisherRequestDTO)
        {
            var publisherDomainModel = new Publisher
            {
                Name = addPublisherRequestDTO.Name
            };
            _dbContext.Publishers.Add(publisherDomainModel);
            await _dbContext.SaveChangesAsync();
            return addPublisherRequestDTO;
        }

        public async Task<PublisherNoIdDTO?> UpdatePublisherByIdAsync(int id, PublisherNoIdDTO publisherNoIdDTO)
        {
            var publisherDomain = await _dbContext.Publishers.FirstOrDefaultAsync(n => n.Id == id);
            if (publisherDomain == null) return null;

            publisherDomain.Name = publisherNoIdDTO.Name;
            await _dbContext.SaveChangesAsync();
            return publisherNoIdDTO;
        }

        public async Task<bool> HasBooksAsync(int id)
        {
            return await _dbContext.Books.AnyAsync(b => b.PublisherID == id);
        }

        public async Task<Publisher?> DeletePublisherByIdAsync(int id)
        {
            var publisherDomain = await _dbContext.Publishers.FirstOrDefaultAsync(n => n.Id == id);
            if (publisherDomain == null) return null;

            _dbContext.Publishers.Remove(publisherDomain);
            await _dbContext.SaveChangesAsync();
            return publisherDomain;
        }

        public async Task<PublisherWithBooksAndAuthorsDTO?> GetBooksByPublisherIdAsync(int id)
        {
            return await _dbContext.Publishers
                .Where(p => p.Id == id)
                .Select(p => new PublisherWithBooksAndAuthorsDTO
                {
                    Name = p.Name,
                    BookAuthors = p.Books.Select(b => new BookAuthorDTO
                    {
                        BookName = b.Title,
                        BookAuthors = b.Book_Authors.Select(ba => ba.Author.FullName).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _dbContext.Publishers.AnyAsync(p =>
                p.Name == name && (excludeId == null || p.Id != excludeId));
        }
    }
}