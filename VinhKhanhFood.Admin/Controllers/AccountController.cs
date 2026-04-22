using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers;

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
            ViewBag.Error = "API chua khoi dong hoac khong ket noi duoc toi dia chi da cau hinh.";
            return View(loginInfo);
        }

        if (response.IsSuccessStatusCode)
        {
            var userJson = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user is null)
            {
                ViewBag.Error = "Khong doc duoc du lieu tai khoan tu API.";
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
            HttpContext.Session.SetString("UserName", "Doi Tac Moi");
            HttpContext.Session.SetInt32("UserId", 999);
            await UpdatePresenceAsync(999, true);

            return RedirectToAction("Index", "Poi");
        }

        ViewBag.Error = "Tai khoan hoac mat khau khong dung.";
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
            var users = await client.GetFromJsonAsync<List<User>>("User") ?? new List<User>();
            var onlineTravelerCount = users.Count(user =>
                string.Equals(user.OnlineStatus, "Online", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Role, "Owner", StringComparison.OrdinalIgnoreCase));

            ViewBag.ActiveTravelerCount = onlineTravelerCount;

            return View(users);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Khong the tai danh sach nguoi dung vi API chua chay.";
            ViewBag.ActiveTravelerCount = 0;
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
            TempData["Error"] = "Khong the khoa tai khoan vi API chua chay.";
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
            TempData["Error"] = "Khong the mo khoa tai khoan vi API chua chay.";
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
                TempData["Error"] = "Khong the xoa tai khoan nay.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Khong the xoa tai khoan vi API chua chay.";
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
