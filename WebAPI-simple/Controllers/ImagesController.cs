using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.Domain;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;   // 10 MB

        private readonly IImageRepository _imageRepository;

        public ImagesController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpPost("Upload")]
        [Authorize(Roles = "Write")]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequestDTO request)
        {
            if (!ValidateFileUpload(request))
            {
                return BadRequest(ModelState);
            }

            var imageDomainModel = new Image
            {
                File = request.File,
                FileExtension = Path.GetExtension(request.File.FileName),
                FileSizeInBytes = request.File.Length,
                FileName = request.FileName,
                FileDescription = request.FileDescription
            };

            var uploaded = await _imageRepository.UploadAsync(imageDomainModel);
            return Ok(uploaded);
        }

        [HttpGet]
        [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> GetAllInfoImages()
        {
            var allImages = await _imageRepository.GetAllInfoImagesAsync();
            return Ok(allImages);
        }

        [HttpGet("Download")]
        [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> DownloadImage([FromQuery] int id)
        {
            var result = await _imageRepository.DownloadFileAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy file" });
            }
            return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
        }

        #region Private methods
        private bool ValidateFileUpload(ImageUploadRequestDTO request)
        {
            if (request.File == null)
            {
                ModelState.AddModelError(nameof(request.File), "File không được để trống");
                return false;
            }

            var extension = Path.GetExtension(request.File.FileName);
            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(request.File), "Chỉ chấp nhận file .jpg, .jpeg, .png");
            }

            if (request.File.Length > MaxFileSizeBytes)
            {
                ModelState.AddModelError(nameof(request.File), "File quá lớn, vui lòng upload file dưới 10MB");
            }

            return ModelState.ErrorCount == 0;
        }
        #endregion
    }
}