using Microsoft.AspNetCore.Mvc;
using VinhKhanhFood.API.Models;

public class PoiController : Controller
{
    private readonly HttpClient _http; // Admin gọi sang API để lấy/lưu dữ liệu

    public PoiController()
    {
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5020/api/") };
    }

    // Hiển thị danh sách POI
    public async Task<IActionResult> Index()
    {
        var locations = await _http.GetFromJsonAsync<List<FoodLocation>>("Food");
        return View(locations);
    }

    // GET: Trang thêm mới POI
    public IActionResult Create() => View();

    // POST: Xử lý thêm mới kèm Upload Audio/Ảnh
    [HttpPost]
    public async Task<IActionResult> Create(FoodLocation model, IFormFile imageFile, IFormFile audioFile)
    {
        // 1. Logic lưu file vào wwwroot/images và wwwroot/audio
        // 2. Gửi dữ liệu model sang API FoodController
        // 3. Redirect về Index
        return RedirectToAction("Index");
    }
}