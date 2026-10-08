using System.Net.Http.Json;
using library_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace library_web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO model)
        {
            var client = _httpClientFactory.CreateClient("BookApi");
            var response = await client.PostAsJsonAsync("api/Auth/Login", model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Sai username hoặc password";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            HttpContext.Session.SetString("JwtToken", result!.JwtToken);
            HttpContext.Session.SetString("Username", model.Username);

            return RedirectToAction("Index", "Books");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}