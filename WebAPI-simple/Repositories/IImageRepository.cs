using WebAPI_simple.Models.Domain;

namespace WebAPI_simple.Repositories
{
    public interface IImageRepository
    {
        Task<Image> UploadAsync(Image image);
        Task<List<Image>> GetAllInfoImagesAsync();
        Task<(byte[] Content, string ContentType, string FileName)?> DownloadFileAsync(int id);
    }
}