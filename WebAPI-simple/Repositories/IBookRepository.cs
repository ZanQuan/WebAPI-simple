using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public interface IBookRepository
    {
        Task<List<BookWithAuthorAndPublisherDTO>> GetAllBooksAsync();
        Task<BookWithAuthorAndPublisherDTO?> GetBookByIdAsync(int id);
        Task<AddBookRequestDTO?> AddBookAsync(AddBookRequestDTO addBookRequestDTO);
        Task<AddBookRequestDTO?> UpdateBookByIdAsync(int id, AddBookRequestDTO bookDTO);
        Task<Book?> DeleteBookByIdAsync(int id);
    }
}