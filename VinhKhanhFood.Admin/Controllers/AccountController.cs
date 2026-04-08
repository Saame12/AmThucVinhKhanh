using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VinhKhanhFood.Admin.Models;
using System.Text;
using System.Net.Http.Json;

namespace VinhKhanhFood.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login() => View();

        /// <summary>
        /// Admin Login
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoginAdmin(string username, string password)
        {
            try
            {
                // Hardcode admin credentials (In production, use secure storage)
                if (username == "admin" && password == "admin123")
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    HttpContext.Session.SetString("UserName", "Administrator");
                    HttpContext.Session.SetInt32("UserId", 0);
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "❌ Tên đăng nhập hoặc mật khẩu Admin không đúng!";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin login");
                ViewBag.Error = "⚠️ Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại!";
                return View("Login");
            }
        }

        /// <summary>
        /// Owner Login
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoginOwner(string username, string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");
                var loginData = new { username, password };
                var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("owner/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LoginOwnerResponse>(jsonContent);

                    HttpContext.Session.SetString("UserRole", "Owner");
                    HttpContext.Session.SetString("UserName", loginResponse.FullName);
                    HttpContext.Session.SetInt32("UserId", loginResponse.Id);

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.OwnerError = "❌ Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản chưa được duyệt!";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during owner login");
                ViewBag.OwnerError = "⚠️ Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại!";
                return View("Login");
            }
        }

        /// <summary>
        /// Owner Registration
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterOwner(
            string username, 
            string password, 
            string email, 
            string phoneNumber,
            string fullName,
            string businessName,
            string businessDescription,
            string address,
            string identificationNumber,
            string taxNumber)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyAPI");

                var registerData = new
                {
                    username,
                    password,
                    email,
                    phoneNumber,
                    fullName,
                    businessName,
                    businessDescription,
                    address,
                    latitude = 0.0,  // Will be set by owner when adding POI
                    longitude = 0.0,
                    identificationNumber,
                    taxNumber
                };

                var jsonContent = JsonConvert.SerializeObject(registerData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("owner/register", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.OwnerSuccess = "✅ Đăng ký thành công! Tài khoản của bạn đang chờ duyệt từ Admin. Vui lòng quay lại để đăng nhập.";
                    return View("Login");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.OwnerError = $"❌ Đăng ký thất bại: {errorContent}";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during owner registration");
                ViewBag.OwnerError = "⚠️ Có lỗi xảy ra khi đăng ký. Vui lòng thử lại!";
                return View("Login");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    // Response DTOs
    public class LoginOwnerResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}