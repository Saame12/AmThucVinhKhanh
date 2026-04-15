using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.Admin.Controllers
{
    public class PoiController : Controller
    {
        private readonly HttpClient _http;

        public PoiController()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5020/api/") };
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var locations = await _http.GetFromJsonAsync<List<FoodLocation>>("Food") ?? new List<FoodLocation>();

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
                return View(new List<FoodLocation>());
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
        public async Task<IActionResult> Create(FoodLocation model, IFormFile imageFile, IFormFile audioViFile, IFormFile audioEnFile, IFormFile audioZhFile)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (imageFile != null)
            {
                model.ImageUrl = await SaveFile(imageFile, "images");
            }

            // Uploaded audio files are treated as professional audio.
            if (audioViFile != null)
            {
                model.AudioUrl = await SaveFile(audioViFile, "audio");
            }

            if (audioEnFile != null)
            {
                model.AudioUrl_EN = await SaveFile(audioEnFile, "audio");
            }

            if (audioZhFile != null)
            {
                model.AudioUrl_ZH = await SaveFile(audioZhFile, "audio");
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
                var data = await _http.GetFromJsonAsync<FoodLocation>($"Food/{id}");
                return View(data);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FoodLocation model, IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (imageFile != null)
            {
                model.ImageUrl = await SaveFile(imageFile, "images");
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
                var poi = await _http.GetFromJsonAsync<FoodLocation>($"Food/{id}");
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
