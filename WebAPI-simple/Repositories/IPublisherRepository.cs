using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;

namespace WebAPI_simple.Repositories
{
    public interface IPublisherRepository
    {
        Task<List<PublisherDTO>> GetAllPublishersAsync();
        Task<PublisherNoIdDTO?> GetPublisherByIdAsync(int id);
        Task<AddPublisherRequestDTO> AddPublisherAsync(AddPublisherRequestDTO addPublisherRequestDTO);
        Task<PublisherNoIdDTO?> UpdatePublisherByIdAsync(int id, PublisherNoIdDTO publisherNoIdDTO);
        Task<bool> HasBooksAsync(int id);
        Task<Publisher?> DeletePublisherByIdAsync(int id);
        Task<PublisherWithBooksAndAuthorsDTO?> GetBooksByPublisherIdAsync(int id);   
    }
}