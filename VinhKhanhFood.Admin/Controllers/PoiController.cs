using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using VinhKhanhFood.Admin.Models;
using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Controllers
{
    public class PoiController : Controller
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public PoiController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _http = httpClientFactory.CreateClient("MyAPI");
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var locations = await _http.GetFromJsonAsync<List<ApiFoodLocation>>("Food") ?? new List<ApiFoodLocation>();

                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetInt32("UserId");

                if (role == "Owner" && userId.HasValue)
                {
                    locations = locations.Where(x => x.OwnerId == userId.Value).ToList();
                }

                return View(locations);
            }
            catch
            {
                return View(new List<ApiFoodLocation>());
            }
        }

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApiFoodLocation model, IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (imageFile != null)
            {
                model.ImageUrl = await SaveFile(imageFile, "images");
            }

            model.Status = "Pending";

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                model.OwnerId = userId.Value;
            }

            var response = await _http.PostAsJsonAsync("Food", model);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var data = await _http.GetFromJsonAsync<ApiFoodLocation>($"Food/{id}");
                return View(data);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            ApiFoodLocation model,
            IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            ApiFoodLocation? existing;
            try
            {
                existing = await _http.GetFromJsonAsync<ApiFoodLocation>($"Food/{model.Id}");
            }
            catch
            {
                existing = null;
            }

            if (existing is null)
            {
                return RedirectToAction(nameof(Index));
            }

            model.OwnerId = existing.OwnerId;
            model.Status = string.IsNullOrWhiteSpace(model.Status) ? existing.Status : model.Status;
            model.Name_EN = string.IsNullOrWhiteSpace(model.Name_EN) ? existing.Name_EN : model.Name_EN;
            model.Name_ZH = string.IsNullOrWhiteSpace(model.Name_ZH) ? existing.Name_ZH : model.Name_ZH;
            model.Description_EN = string.IsNullOrWhiteSpace(model.Description_EN) ? existing.Description_EN : model.Description_EN;
            model.Description_ZH = string.IsNullOrWhiteSpace(model.Description_ZH) ? existing.Description_ZH : model.Description_ZH;
            if (Math.Abs(model.Latitude) < double.Epsilon)
            {
                model.Latitude = existing.Latitude;
            }

            if (Math.Abs(model.Longitude) < double.Epsilon)
            {
                model.Longitude = existing.Longitude;
            }

            if (imageFile != null)
            {
                model.ImageUrl = await SaveFile(imageFile, "images");
            }
            else
            {
                model.ImageUrl = existing.ImageUrl;
            }

            var response = await _http.PutAsJsonAsync($"Food/{model.Id}", model);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            await _http.DeleteAsync($"Food/{id}");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Approve(int id)
        {
            await _http.PutAsync($"Food/approve/{id}", null);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reject(int id)
        {
            await _http.PutAsync($"Food/reject/{id}", null);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Qr(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var poi = await _http.GetFromJsonAsync<ApiFoodLocation>($"Food/{id}");
                if (poi is null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(poi);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "VinhKhanhFood.API", "wwwroot", folder, fileName);

            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }
    }
}
