using System.ComponentModel.DataAnnotations;

namespace WebAPI_simple.Models.DTO
{
    public class AddBookRequestDTO
    {
        [Required(ErrorMessage = "Title không được để trống")]
        [MinLength(10, ErrorMessage = "Title tối thiểu 10 ký tự")]
        [RegularExpression(@"^[\p{L}\p{N}\s]+$", ErrorMessage = "Title không được chứa ký tự đặc biệt")]
        public string? Title { get; set; }

        public string? Description { get; set; }   
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }

        [Range(0, 5, ErrorMessage = "Rate phải từ 0 đến 5")]
        public int? Rate { get; set; }

        [Required(ErrorMessage = "Genre không được để trống")]
        public string? Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PublisherID không hợp lệ")]
        public int PublisherID { get; set; }

        [MinLength(1, ErrorMessage = "Sách phải có ít nhất 1 tác giả")]
        public List<int> AuthorIds { get; set; } = new List<int>();
    }
}