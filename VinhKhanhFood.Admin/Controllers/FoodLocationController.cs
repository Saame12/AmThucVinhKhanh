using Microsoft.AspNetCore.Mvc;

namespace VinhKhanhFood.Admin.Controllers
{
    public class FoodLocationController : Controller
    {
        /// <summary>
        /// Display pending food locations for admin approval
        /// View loads data via API calls (JavaScript fetch)
        /// </summary>
        public IActionResult PendingApprovals()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Index", "Home");

            return View();
        }
    }
}
