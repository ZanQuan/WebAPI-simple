using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public interface IPublisherRepository
    {
        Task<List<PublisherDTO>> GetAllPublishersAsync(
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 1000);
        Task<PublisherNoIdDTO?> GetPublisherByIdAsync(int id);
        Task<AddPublisherRequestDTO> AddPublisherAsync(AddPublisherRequestDTO addPublisherRequestDTO);
        Task<PublisherNoIdDTO?> UpdatePublisherByIdAsync(int id, PublisherNoIdDTO publisherNoIdDTO);
        Task<bool> HasBooksAsync(int id);
        Task<Publisher?> DeletePublisherByIdAsync(int id);
        Task<PublisherWithBooksAndAuthorsDTO?> GetBooksByPublisherIdAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}