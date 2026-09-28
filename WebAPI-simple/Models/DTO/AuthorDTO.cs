using System.ComponentModel.DataAnnotations;

namespace WebAPI_simple.Models.DTO
{
    public class AuthorDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }

    public class AuthorNoIdDTO
    {
        [Required(ErrorMessage = "FullName không được để trống")]
        [MinLength(3, ErrorMessage = "FullName tối thiểu 3 ký tự")]
        public string FullName { get; set; }
    }

    public class AuthorWithBooksDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public List<string> BookTitles { get; set; }
    }
}