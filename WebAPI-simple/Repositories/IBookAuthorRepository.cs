using WebAPI_simple.Models.Domain;

namespace WebAPI_simple.Repositories
{
    public interface IBookAuthorRepository
    {
        Task<bool> BookExistsAsync(int bookId);
        Task<bool> AuthorExistsAsync(int authorId);
        Task<bool> RelationExistsAsync(int bookId, int authorId);
        Task<Book_Author> AddAsync(int bookId, int authorId);
    }
}