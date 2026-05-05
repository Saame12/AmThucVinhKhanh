using Microsoft.AspNetCore.Mvc;

namespace VinhKhanhFood.Admin.Controllers;

public class OwnerController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrWhiteSpace(role))
        {
            return RedirectToAction("Login", "Account");
        }

        if (!string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
}
