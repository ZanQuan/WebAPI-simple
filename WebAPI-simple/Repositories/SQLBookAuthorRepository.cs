using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;

namespace WebAPI_simple.Repositories
{
    public class SQLBookAuthorRepository : IBookAuthorRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLBookAuthorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> BookExistsAsync(int bookId)
            => await _dbContext.Books.AnyAsync(b => b.Id == bookId);

        public async Task<bool> AuthorExistsAsync(int authorId)
            => await _dbContext.Authors.AnyAsync(a => a.Id == authorId);

        public async Task<bool> RelationExistsAsync(int bookId, int authorId)
            => await _dbContext.Books_Authors.AnyAsync(ba => ba.BookId == bookId && ba.AuthorId == authorId);

        public async Task<Book_Author> AddAsync(int bookId, int authorId)
        {
            var entity = new Book_Author { BookId = bookId, AuthorId = authorId };
            _dbContext.Books_Authors.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}