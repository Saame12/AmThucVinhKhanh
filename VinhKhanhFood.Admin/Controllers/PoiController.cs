using Microsoft.AspNetCore.Mvc;
using VinhKhanhFood.API.Models;
using System.Net.Http.Json;

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

            // Bước B: Gửi dữ liệu sang API
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
    }
}