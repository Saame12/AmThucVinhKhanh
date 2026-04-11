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
            // 🔥 gọi API history
            var data = await _http.GetFromJsonAsync<List<UsageHistory>>("Food/history");

            // 🔥 NEW: filter hôm nay
            var today = DateTime.Today;

            data = data
                .Where(x => x.CreatedAt.Date == today)
                .ToList();

            return View(data);
        }
    }
}
