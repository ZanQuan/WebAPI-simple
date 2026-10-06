using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebAPI_simple.Models.DTO
{
    public class ImageUploadRequestDTO
    {
        [Required(ErrorMessage = "File không được để trống")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "FileName không được để trống")]
        public string FileName { get; set; }

        public string? FileDescription { get; set; }
    }
}