using Microsoft.AspNetCore.Mvc;
using VinhKhanhFood.Admin.Models;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VinhKhanhFood.Admin.Controllers
{
    public class OwnerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OwnerController> _logger;

        public OwnerController(IHttpClientFactory httpClientFactory, ILogger<OwnerController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("MyAPI");
            _logger = logger;
        }

    // View Models used by OwnerController
    public class OwnerViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string? BusinessDescription { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? TaxNumber { get; set; }
    }

        // 1. Danh sách quán đã đăng ký (Đã duyệt)
        public async Task<IActionResult> Index()
        {
            var owners = await GetOwnersFromApi("admin/all-owners");
            // Lọc ra những quán có status là Approved nếu API trả về chung
            return View(owners.Where(x => x.Status == "Approved").ToList());
        }

        // 2. Danh sách quán đang chờ duyệt
        public async Task<IActionResult> Pending()
        {
            var owners = await GetOwnersFromApi("admin/pending-owners");
            return View(owners);
        }

        // 3. Giao diện thêm quán mới (GET)
        public IActionResult Create()
        {
            var ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (ownerId == 0) return RedirectToLogin();

            return View(new AddFoodLocationViewModel { OwnerId = ownerId });
        }

        // 4. Xử lý thêm quán mới (POST)
        [HttpPost]
        public async Task<IActionResult> Create(AddFoodLocationViewModel model)
        {
            var ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (ownerId == 0) return RedirectToLogin();

            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync($"owner/{ownerId}/foodlocation", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Đã gửi yêu cầu. Vui lòng chờ admin duyệt.";
                return RedirectToAction("Pending");
            }

            ViewBag.Error = "Không thể thêm quán. Vui lòng thử lại.";
            return View(model);
        }

        // GET: Owner/FoodLocations - Danh sách quán của owner
        public IActionResult FoodLocations()
        {
            var ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (ownerId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // GET: Owner/Profile - Xem/edit hồ sơ
        public IActionResult Profile()
        {
            var ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (ownerId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // --- Các phương thức hỗ trợ (Private Helpers) ---

        private async Task<List<OwnerViewModel>> GetOwnersFromApi(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<OwnerViewModel>>() ?? new List<OwnerViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gọi API: {endpoint}");
            }
            return new List<OwnerViewModel>();
        }

        private IActionResult RedirectToLogin()
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }
    }
}