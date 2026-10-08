namespace library_web.Models.DTO
{
    public class LoginRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string JwtToken { get; set; }
    }
}