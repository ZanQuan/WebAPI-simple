using System.ComponentModel.DataAnnotations;

namespace WebAPI_simple.Models.DTO
{
    public class RegisterRequestDTO
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [EmailAddress(ErrorMessage = "Username phải là email hợp lệ")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(6, ErrorMessage = "Password tối thiểu 6 ký tự")]
        public string Password { get; set; }

        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}