using Microsoft.EntityFrameworkCore;
using WebAPI_simple.Data;
using WebAPI_simple.Models.Domain;

namespace WebAPI_simple.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LocalImageRepository(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<Image> UploadAsync(Image image)
        {
            var imagesFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");
            Directory.CreateDirectory(imagesFolder);   // tự tạo nếu chưa có

            var localFilePath = Path.Combine(imagesFolder, $"{image.FileName}{image.FileExtension}");

            using (var stream = new FileStream(localFilePath, FileMode.Create))
            {
                await image.File!.CopyToAsync(stream);
            }

            var httpContext = _httpContextAccessor.HttpContext!;
            image.FilePath =
                $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}/Images/{image.FileName}{image.FileExtension}";

            _dbContext.Images.Add(image);
            await _dbContext.SaveChangesAsync();

            return image;
        }

        public async Task<List<Image>> GetAllInfoImagesAsync()
        {
            return await _dbContext.Images.ToListAsync();
        }

        public async Task<(byte[] Content, string ContentType, string FileName)?> DownloadFileAsync(int id)
        {
            var image = await _dbContext.Images.FirstOrDefaultAsync(x => x.Id == id);
            if (image == null) return null;

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images",
                $"{image.FileName}{image.FileExtension}");
            if (!File.Exists(path)) return null;

            var content = await File.ReadAllBytesAsync(path);
            return (content, "application/octet-stream", image.FileName + image.FileExtension);
        }
    }
}