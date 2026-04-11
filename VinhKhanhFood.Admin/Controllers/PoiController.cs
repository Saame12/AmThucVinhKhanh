using Microsoft.AspNetCore.Mvc;
using VinhKhanhFood.API.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http; // 🔥 FIX LỖI Session
using System.Linq; // 🔥 NEW (bắt buộc cho Where)
namespace VinhKhanhFood.Admin.Controllers
{
    public class PoiController : Controller
    {
        private readonly HttpClient _http;

        public PoiController()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5020/api/") };
        }

        // 1. Hiển thị danh sách POI (Đã thêm kiểm tra Session)
        public async Task<IActionResult> Index()
        {
            // Kiểm tra xem User đã đăng nhập chưa
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var locations = await _http.GetFromJsonAsync<List<FoodLocation>>("Food");

                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetInt32("UserId");
                // 🔥 DEBUG: kiểm tra UserId
                Console.WriteLine($"USER ID: {userId}");
                Console.WriteLine($"ROLE: {role}");

                if (role == "Owner" && userId.HasValue)
                {
                    locations = locations.Where(x => x.OwnerId == userId.Value).ToList();
                }

                return View(locations);
            }
            catch
            {
                // Nếu API lỗi, vẫn trả về View với danh sách rỗng để không sập trang
                return View(new List<FoodLocation>());
            }
        }

        // 2. Trang thêm mới (Nên thêm kiểm tra Session ở đây luôn)
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // 3. Xử lý lưu dữ liệu
        [HttpPost]
        public async Task<IActionResult> Create(FoodLocation model, IFormFile imageFile, IFormFile audioViFile, IFormFile audioEnFile, IFormFile audioZhFile)
        {
            // Kiểm tra bảo mật khi submit form
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            // Bước A: Xử lý lưu file vật lý
            if (imageFile != null) model.ImageUrl = await SaveFile(imageFile, "images");
            if (audioViFile != null) model.AudioUrl = await SaveFile(audioViFile, "audio");
            if (audioEnFile != null) model.AudioUrl_EN = await SaveFile(audioEnFile, "audio");
            if (audioZhFile != null) model.AudioUrl_ZH = await SaveFile(audioZhFile, "audio");

            // 🔥 NEW: AUTO GENERATE AUDIO nếu không upload file
            if (audioViFile == null && !string.IsNullOrEmpty(model.Description))
            {
                model.AudioUrl = GenerateFakeAudio(model.Description, "vi");
            }

            if (audioEnFile == null && !string.IsNullOrEmpty(model.Description_EN))
            {
                model.AudioUrl_EN = GenerateFakeAudio(model.Description_EN, "en");
            }

            if (audioZhFile == null && !string.IsNullOrEmpty(model.Description_ZH))
            {
                model.AudioUrl_ZH = GenerateFakeAudio(model.Description_ZH, "zh");
            }
            // 🔥 NEW: mặc định Pending
            model.Status = "Pending";
            // Bước B: Gửi dữ liệu sang API
            // 🔥 NEW: Gán OwnerId (safe)
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

        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "VinhKhanhFood.API", "wwwroot", folder, fileName);

            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
        // 🔥 NEW: GET Edit
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

        // 🔥 NEW: POST Edit
        [HttpPost]
        public async Task<IActionResult> Edit(FoodLocation model, IFormFile imageFile)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            // 🔥 NEW: update image nếu có
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

        // 🔥 NEW: DELETE
        public async Task<IActionResult> Delete(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Account");
            }

            await _http.DeleteAsync($"Food/{id}");
            return RedirectToAction(nameof(Index));
        }
        // 🔥 NEW: Approve POI
        public async Task<IActionResult> Approve(int id)
        {
            await _http.PutAsync($"Food/approve/{id}", null);
            return RedirectToAction(nameof(Index));
        }

        // 🔥 NEW: Reject POI
        public async Task<IActionResult> Reject(int id)
        {
            await _http.PutAsync($"Food/reject/{id}", null);
            return RedirectToAction(nameof(Index));
        }
        // 🔥 NEW: Fake TTS (demo)
        private string GenerateFakeAudio(string text, string lang)
        {
            // NOTE: sau này thay bằng gọi API thật (Google TTS / Azure)
            return $"auto_{lang}_{Guid.NewGuid()}.mp3";
        }

        
    }
}