using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers
{
    public class UsageController : Controller
    {
        private readonly HttpClient _http;

        public UsageController()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5020/api/") };
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _http.GetFromJsonAsync<List<UsageHistory>>("Food/history");
                return View(data ?? new List<UsageHistory>());
            }
            catch
            {
                TempData["Error"] = "Không thể tải lịch sử sử dụng vì API chưa chạy.";
                return View(new List<UsageHistory>());
            }
        }
    }
}
