using System.ComponentModel.DataAnnotations;

namespace library_web.Models.DTO
{
    public class AddBookDTO
    {
        [Required(ErrorMessage = "Title không được để trống")]
        [MinLength(10, ErrorMessage = "Title tối thiểu 10 ký tự")]
        public string Title { get; set; }

        public string? Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public int? Rate { get; set; }
        public string? Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public int PublisherID { get; set; }
        public List<int> AuthorIds { get; set; } = new();
    }
}