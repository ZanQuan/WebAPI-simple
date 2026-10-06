using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI_simple.Models.DTO;
using WebAPI_simple.Repositories;

namespace WebAPI_simple.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        // POST: /api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDTO.Username,
                Email = registerRequestDTO.Username
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDTO.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors.Select(e => e.Description));
            }

            if (registerRequestDTO.Roles.Any())
            {
                identityResult = await _userManager.AddToRolesAsync(identityUser, registerRequestDTO.Roles);
                if (!identityResult.Succeeded)
                {
                    return BadRequest(identityResult.Errors.Select(e => e.Description));
                }
            }

            return Ok(new { message = "Đăng ký thành công, hãy đăng nhập" });
        }

        // POST: /api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.Username);
            if (user == null)
            {
                return BadRequest(new { message = "Username hoặc password không đúng" });
            }

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if (!checkPasswordResult)
            {
                return BadRequest(new { message = "Username hoặc password không đúng" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = _tokenRepository.CreateJWTToken(user, roles.ToList());

            return Ok(new LoginResponseDTO { JwtToken = jwtToken });
        }
    }
}