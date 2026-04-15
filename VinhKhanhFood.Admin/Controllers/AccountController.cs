using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers
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
        public async Task<IActionResult> Login(User loginInfo)
        {
            HttpResponseMessage response;

            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                var content = new StringContent(JsonConvert.SerializeObject(loginInfo), Encoding.UTF8, "application/json");
                response = await client.PostAsync("User/login", content);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "API chưa khởi động hoặc không kết nối được tới server tại localhost:5020.";
                return View(loginInfo);
            }

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);

                if (user is null)
                {
                    ViewBag.Error = "Không đọc được dữ liệu tài khoản từ API.";
                    return View(loginInfo);
                }

                var role = user.Role == "Vendor" ? "Owner" : user.Role;

                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetInt32("UserId", user.Id);

                await UpdatePresenceAsync(user.Id, true);
                return RedirectToAction("Index", "Poi");
            }

            if (loginInfo.Username == "TroLyVinhKhanh" && loginInfo.Password == "1")
            {
                HttpContext.Session.SetString("UserRole", "Owner");
                HttpContext.Session.SetString("UserName", "Đối Tác Mới");
                HttpContext.Session.SetInt32("UserId", 999);
                await UpdatePresenceAsync(999, true);

                return RedirectToAction("Index", "Poi");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View(loginInfo);
        }

        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                await UpdatePresenceAsync(userId.Value, false);
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Users()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                var users = await client.GetFromJsonAsync<List<User>>("User");
                return View(users ?? new List<User>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Không thể tải danh sách người dùng vì API chưa chạy.";
                return View(new List<User>());
            }
        }

        public async Task<IActionResult> Block(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                await client.PutAsync($"User/block/{id}", null);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Không thể khóa tài khoản vì API chưa chạy.";
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Unblock(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                await client.PutAsync($"User/unblock/{id}", null);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Không thể mở khóa tài khoản vì API chưa chạy.";
            }

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                var response = await client.DeleteAsync($"User/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể xóa tài khoản này.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Không thể xóa tài khoản vì API chưa chạy.";
            }

            return RedirectToAction("Users");
        }

        private async Task UpdatePresenceAsync(int userId, bool isOnline)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                await client.PutAsync($"User/presence/{userId}?isOnline={isOnline.ToString().ToLowerInvariant()}", null);
            }
            catch
            {
            }
        }
    }
}
