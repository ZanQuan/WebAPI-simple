using System.ComponentModel.DataAnnotations;

namespace WebAPI_simple.Models.DTO
{
    public class AddBookAuthorRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "BookId không hợp lệ")]
        public int BookId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "AuthorId không hợp lệ")]
        public int AuthorId { get; set; }
    }
}