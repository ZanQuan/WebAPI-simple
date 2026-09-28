using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublishersController(IPublisherRepository publisherRepository)
        {
            _publisherRepository = publisherRepository;
        }

        [HttpGet("get-all-publisher")]
        public async Task<IActionResult> GetAllPublisher()
        {
            var allPublishers = await _publisherRepository.GetAllPublishersAsync();
            return Ok(allPublishers);
        }

        [HttpGet("get-publisher-by-id/{id:int}")]
        public async Task<IActionResult> GetPublisherById(int id)
        {
            var publisherWithId = await _publisherRepository.GetPublisherByIdAsync(id);
            if (publisherWithId == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }
            return Ok(publisherWithId);
        }

        [HttpPost("add-publisher")]
        public async Task<IActionResult> AddPublisher([FromBody] AddPublisherRequestDTO addPublisherRequestDTO)
        {
            addPublisherRequestDTO.Name = addPublisherRequestDTO.Name.Trim();
            if (await _publisherRepository.NameExistsAsync(addPublisherRequestDTO.Name))
            {
                return Conflict(new { message = "Tên NXB đã tồn tại" });
            }
            var publisherAdd = await _publisherRepository.AddPublisherAsync(addPublisherRequestDTO);
            return Ok(publisherAdd);
        }

        [HttpPut("update-publisher-by-id/{id:int}")]
        public async Task<IActionResult> UpdatePublisherById(int id, [FromBody] PublisherNoIdDTO publisherDTO)
        {
            publisherDTO.Name = publisherDTO.Name.Trim();
            if (await _publisherRepository.NameExistsAsync(publisherDTO.Name, id))
            {
                return Conflict(new { message = "Tên NXB đã tồn tại" });
            }
            var publisherUpdate = await _publisherRepository.UpdatePublisherByIdAsync(id, publisherDTO);
            if (publisherUpdate == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }
            return Ok(publisherUpdate);
        }

        [HttpDelete("delete-publisher-by-id/{id:int}")]
        public async Task<IActionResult> DeletePublisherById(int id)
        {
            if (await _publisherRepository.HasBooksAsync(id))
            {
                return Conflict(new { message = "NXB này còn sách, hãy xóa hoặc chuyển sách trước" });
            }

            var publisherDelete = await _publisherRepository.DeletePublisherByIdAsync(id);
            if (publisherDelete == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }
            return Ok(new { message = "Đã xóa NXB" });
        }

        [HttpGet("{id:int}/books")]
        public async Task<IActionResult> GetBooksByPublisherId(int id)
        {
            var result = await _publisherRepository.GetBooksByPublisherIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy NXB" });
            }
            return Ok(result);
        }
    }
}