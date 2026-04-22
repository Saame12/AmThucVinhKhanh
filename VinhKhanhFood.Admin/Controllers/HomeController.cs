using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VinhKhanhFood.Admin.Models;
using VinhKhanhFood.Admin.Services;

namespace VinhKhanhFood.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SetLanguage(string lang)
        {
            if (!string.IsNullOrEmpty(lang))
            {
                LocalizationService.SetLanguage(lang);
                HttpContext.Session.SetString("CurrentLanguage", lang);
            }

            var returnUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home") ?? "/";
            }

            return Redirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
