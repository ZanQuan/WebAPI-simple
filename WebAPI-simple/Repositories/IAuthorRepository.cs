using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public interface IAuthorRepository
    {
        Task<List<AuthorDTO>> GetAllAuthorsAsync(
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 1000);
        Task<AuthorNoIdDTO?> GetAuthorByIdAsync(int id);
        Task<AddAuthorRequestDTO> AddAuthorAsync(AddAuthorRequestDTO addAuthorRequestDTO);
        Task<AuthorNoIdDTO?> UpdateAuthorByIdAsync(int id, AuthorNoIdDTO authorNoIdDTO);
        Task<Author?> DeleteAuthorByIdAsync(int id);
        Task<AuthorWithBooksDTO?> GetBooksByAuthorIdAsync(int id);
        Task<bool> HasBooksAsync(int id);
    }
}