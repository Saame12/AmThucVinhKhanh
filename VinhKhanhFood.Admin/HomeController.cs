using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using VinhKhanhFood.Admin.Models;
using static System.Net.WebRequestMethods;

namespace VinhKhanhFood.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AccountController(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(User loginInfo)
        {
            var client = _httpClientFactory.CreateClient("MyAPI");
            var content = new StringContent(JsonConvert.SerializeObject(loginInfo), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("User/login", content);

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);

                // 🔥 NEW: Chuẩn hóa role
                var role = user.Role == "Vendor" ? "Owner" : user.Role;

                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetInt32("UserId", user.Id);

                return RedirectToAction("Index", "Poi");
            }

            // 🔥 NEW: fallback account (Owner test)
            if (loginInfo.Username == "TroLyVinhKhanh" && loginInfo.Password == "1")
            {
                HttpContext.Session.SetString("UserRole", "Owner");
                HttpContext.Session.SetString("UserName", "Đối Tác Mới");
                HttpContext.Session.SetInt32("UserId", 999);

                return RedirectToAction("Index", "Poi");
            }

            // 🔥 NEW: bắt buộc phải có
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        // USERR-DU KHACH
        // 🔥 NEW: Danh sách user
        // 🔥 NEW: Danh sách user
        public async Task<IActionResult> Users()
        {
            var client = _httpClientFactory.CreateClient("MyAPI");

            var users = await client.GetFromJsonAsync<List<User>>("User");

            return View(users);
        }

        // 🔥 Block
        public async Task<IActionResult> Block(int id)
        {
            var client = _httpClientFactory.CreateClient("MyAPI");

            await client.PutAsync($"User/block/{id}", null);

            return RedirectToAction("Users"); // 🔥 QUAN TRỌNG
        }

        // 🔥 Unblock
        public async Task<IActionResult> Unblock(int id)
        {
            var client = _httpClientFactory.CreateClient("MyAPI");

            await client.PutAsync($"User/unblock/{id}", null);

            return RedirectToAction("Users"); // 🔥 QUAN TRỌNG
        }
    }
}