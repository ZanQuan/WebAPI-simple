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
        Task<bool> PublisherExistsAsync(int publisherId);
        Task<List<int>> GetMissingAuthorIdsAsync(List<int> authorIds);
        Task<bool> TitleExistsInPublisherAsync(string title, int publisherId, int? excludeBookId = null);
        Task<List<int>> GetAuthorIdsOverLimitAsync(List<int> authorIds, int maxBooks, int? excludeBookId = null);
        Task<int> CountBooksByPublisherInYearAsync(int publisherId, int year, int? excludeBookId = null);
    }
}