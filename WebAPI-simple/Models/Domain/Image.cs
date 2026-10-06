using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace WebAPI_simple.Models.Domain
{
    public class Image
    {
        public int Id { get; set; }

        [NotMapped]   
        public IFormFile? File { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string? FileDescription { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public long FileSizeInBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}