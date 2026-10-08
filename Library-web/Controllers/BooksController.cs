using System.Net.Http.Headers;
using System.Net.Http.Json;
using library_web.CustomActionFilters;
using library_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace library_web.Controllers
{
    [SessionAuthorize]
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Gắn kèm JWT token vào mọi request gọi tới API
        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("BookApi");
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // GET: /Books
        public async Task<IActionResult> Index(string? filterOn, string? filterQuery, string? sortBy, bool isAscending = true)
        {
            var response = new List<BookDTO>();
            try
            {
                var client = CreateAuthorizedClient();
                var url = $"api/Books/get-all-books?filterOn={filterOn}&filterQuery={filterQuery}&sortBy={sortBy}&isAscending={isAscending}";
                var httpResponse = await client.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadFromJsonAsync<List<BookDTO>>() ?? new List<BookDTO>();
            }
            catch (Exception)
            {
                return View("Error");
            }
            return View(response);
        }

        // GET: /Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = CreateAuthorizedClient();
                var httpResponse = await client.GetAsync($"api/Books/get-book-by-id/{id}");
                httpResponse.EnsureSuccessStatusCode();
                var book = await httpResponse.Content.ReadFromJsonAsync<BookDTO>();
                return View(book);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // GET: /Books/AddBook
        [HttpGet]
        public async Task<IActionResult> AddBook()
        {
            await LoadAuthorsAndPublishers();
            return View();
        }

        // POST: /Books/AddBook
        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAuthorsAndPublishers();
                return View(model);
            }

            var client = CreateAuthorizedClient();
            var httpResponse = await client.PostAsJsonAsync("api/Books/add-book", model);

            if (!httpResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = await httpResponse.Content.ReadAsStringAsync();
                await LoadAuthorsAndPublishers();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: /Books/EditBook/5
        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            var client = CreateAuthorizedClient();
            var httpResponse = await client.GetAsync($"api/Books/get-book-by-id/{id}");
            httpResponse.EnsureSuccessStatusCode();
            var book = await httpResponse.Content.ReadFromJsonAsync<BookDTO>();

            var model = new AddBookDTO
            {
                Title = book!.Title!,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.DateRead,
                Rate = book.Rate,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherID = book.PublisherID,
                AuthorIds = book.AuthorIds
            };

            ViewBag.BookId = id;
            await LoadAuthorsAndPublishers();
            return View(model);
        }

        // POST: /Books/EditBook/5
        [HttpPost]
        public async Task<IActionResult> EditBook(int id, AddBookDTO model)
        {
            var client = CreateAuthorizedClient();
            var httpResponse = await client.PutAsJsonAsync($"api/Books/update-book-by-id/{id}", model);

            if (!httpResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = await httpResponse.Content.ReadAsStringAsync();
                ViewBag.BookId = id;
                await LoadAuthorsAndPublishers();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // POST: /Books/DeleteBook/5
        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var client = CreateAuthorizedClient();
            await client.DeleteAsync($"api/Books/delete-book-by-id/{id}");
            return RedirectToAction("Index");
        }

        private async Task LoadAuthorsAndPublishers()
        {
            var client = CreateAuthorizedClient();

            var authorsResponse = await client.GetAsync("api/Authors/get-all-author");
            authorsResponse.EnsureSuccessStatusCode();
            ViewBag.ListAuthor = await authorsResponse.Content.ReadFromJsonAsync<List<AuthorDTO>>();

            var publishersResponse = await client.GetAsync("api/Publishers/get-all-publisher");
            publishersResponse.EnsureSuccessStatusCode();
            ViewBag.ListPublisher = await publishersResponse.Content.ReadFromJsonAsync<List<PublisherDTO>>();
        }
    }
}