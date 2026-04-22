using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using VinhKhanhFood.Admin.Models;
using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Controllers;

public class PaymentController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private const decimal PublicPoiPrice = 10000m;
    private const string PublicGuestCookieName = "vk_public_payment_guest";

    public PaymentController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? poiId, string? status)
    {
        var resolvedEnd = endDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Today.AddDays(1).AddTicks(-1);
        var resolvedStart = startDate?.Date ?? DateTime.Today.AddDays(-6);

        var model = new PaymentDashboardViewModel
        {
            StartDate = resolvedStart,
            EndDate = resolvedEnd,
            SelectedPoiId = poiId,
            SelectedStatus = status
        };

        try
        {
            var client = _httpClientFactory.CreateClient("MyAPI");
            model.Pois = await client.GetFromJsonAsync<List<FoodLocation>>("Food") ?? [];

            var dashboardUrl = $"Payment/dashboard?startDate={resolvedStart:O}&endDate={resolvedEnd:O}";
            if (poiId.HasValue)
            {
                dashboardUrl += $"&poiId={poiId.Value}";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                dashboardUrl += $"&status={Uri.EscapeDataString(status)}";
            }

            using var response = await client.GetAsync(dashboardUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;

            model.TotalTransactions = root.GetProperty("totalTransactions").GetInt32();
            model.PaidTransactions = root.GetProperty("paidTransactions").GetInt32();
            model.PendingTransactions = root.GetProperty("pendingTransactions").GetInt32();
            model.FailedTransactions = root.GetProperty("failedTransactions").GetInt32();
            model.ReconciledTransactions = root.GetProperty("reconciledTransactions").GetInt32();
            model.UnlockedPurchases = root.GetProperty("unlockedPurchases").GetInt32();
            model.TotalRevenue = root.GetProperty("totalRevenue").GetDecimal();
            model.AverageOrderValue = root.GetProperty("averageOrderValue").GetDecimal();
            model.TopPois = DeserializeList<PaymentPoiStat>(root.GetProperty("topPois"));
            model.ProviderBreakdown = DeserializeList<PaymentProviderStat>(root.GetProperty("providerBreakdown"));
            model.Transactions = DeserializeList<PaymentTransaction>(root.GetProperty("transactions"));
        }
        catch (HttpRequestException)
        {
            if (UseDemoDashboardFallback())
            {
                PopulateDemoDashboard(model);
                TempData["Success"] = "QR Payment Dashboard dang hien du lieu demo vi API local chua chay.";
            }
            else
            {
                TempData["Error"] = "Khong the tai QR Payment Dashboard vi API chua chay.";
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Reconcile(int id, DateTime startDate, DateTime endDate, int? poiId, string? status)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MyAPI");
            await client.PutAsync($"Payment/reconcile/{id}", null);
            TempData["Success"] = "Da doi soat mock thanh cong.";
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Khong the doi soat giao dich vi API chua chay.";
        }

        return RedirectToAction(nameof(Index), new
        {
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            poiId,
            status
        });
    }

    [HttpGet("/Payment/Poi/{id:int}")]
    public async Task<IActionResult> Poi(int id, string? message = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MyAPI");
            var poi = await client.GetFromJsonAsync<ApiFoodLocation>($"Food/{id}");
            if (poi is null)
            {
                return NotFound();
            }

            var guestId = EnsurePublicGuestId();
            var access = await client.GetFromJsonAsync<PaymentAccessDto>($"Payment/access?poiId={id}&guestId={Uri.EscapeDataString(guestId)}");

            var model = new PublicPoiPaymentViewModel
            {
                Poi = poi,
                FixedAmount = PublicPoiPrice,
                HasPaidAccess = access?.HasAccess ?? false,
                GuestId = guestId,
                AssetBaseUrl = GetAssetBaseUrl(client.BaseAddress),
                PaymentMessage = message
            };

            return View(model);
        }
        catch (HttpRequestException)
        {
            return View(new PublicPoiPaymentViewModel
            {
                FixedAmount = PublicPoiPrice,
                PaymentMessage = "Khong the ket noi API de tai trang public nay."
            });
        }
    }

    [HttpPost("/Payment/Poi/{id:int}/unlock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockPoi(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MyAPI");
            var guestId = EnsurePublicGuestId();

            var response = await client.PostAsJsonAsync("Payment/mock-checkout", new
            {
                PoiId = id,
                Amount = PublicPoiPrice,
                GuestId = guestId,
                Provider = "ExternalQR-Web"
            });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Poi), new { id, message = "paid" });
            }

            return RedirectToAction(nameof(Poi), new { id, message = "payment-error" });
        }
        catch (HttpRequestException)
        {
            return RedirectToAction(nameof(Poi), new { id, message = "server-error" });
        }
    }

    private static List<T> DeserializeList<T>(JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
    }

    private string EnsurePublicGuestId()
    {
        var guestId = Request.Cookies[PublicGuestCookieName];
        if (!string.IsNullOrWhiteSpace(guestId))
        {
            return guestId;
        }

        guestId = $"web-{Guid.NewGuid():N}"[..16];
        Response.Cookies.Append(PublicGuestCookieName, guestId, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return guestId;
    }

    private static string GetAssetBaseUrl(Uri? baseAddress)
    {
        var raw = (baseAddress?.ToString() ?? "http://localhost:5020/api/").TrimEnd('/');
        return raw.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? raw[..^4]
            : raw;
    }

    private bool UseDemoDashboardFallback() =>
        _configuration.GetValue("DemoSettings:UseMockPaymentDashboardData", true);

    private void PopulateDemoDashboard(PaymentDashboardViewModel model)
    {
        var samplePois = new List<FoodLocation>
        {
            new() { Id = 1, Name = "Oc Dao Vinh Khanh" },
            new() { Id = 2, Name = "Hai San Nuong 198" },
            new() { Id = 3, Name = "Quan Soup Cua Dem" }
        };

        var demoTransactions = new List<PaymentTransaction>
        {
            new()
            {
                Id = 1001,
                TransactionCode = "QR-DEMO-1001",
                PoiId = 1,
                PoiName = samplePois[0].Name,
                GuestId = "web-demo-a1",
                PurchaserDisplayName = "guid-web-demo-a1",
                CustomerLabel = "guest:web-demo-a1",
                Amount = PublicPoiPrice,
                Provider = "ExternalQR-Web",
                Status = "Paid",
                CreatedAt = DateTime.Now.AddMinutes(-45),
                PaidAt = DateTime.Now.AddMinutes(-44)
            },
            new()
            {
                Id = 1002,
                TransactionCode = "QR-DEMO-1002",
                PoiId = 1,
                PoiName = samplePois[0].Name,
                GuestId = "web-demo-b2",
                PurchaserDisplayName = "guid-web-demo-b2",
                CustomerLabel = "guest:web-demo-b2",
                Amount = PublicPoiPrice,
                Provider = "ExternalQR-Web",
                Status = "Paid",
                CreatedAt = DateTime.Now.AddMinutes(-30),
                PaidAt = DateTime.Now.AddMinutes(-28),
                ReconciledAt = DateTime.Now.AddMinutes(-10)
            },
            new()
            {
                Id = 1003,
                TransactionCode = "QR-DEMO-1003",
                PoiId = 2,
                PoiName = samplePois[1].Name,
                GuestId = "web-demo-c3",
                PurchaserDisplayName = "guid-web-demo-c3",
                CustomerLabel = "guest:web-demo-c3",
                Amount = PublicPoiPrice,
                Provider = "MockQR",
                Status = "Pending",
                CreatedAt = DateTime.Now.AddMinutes(-18)
            },
            new()
            {
                Id = 1004,
                TransactionCode = "QR-DEMO-1004",
                PoiId = 3,
                PoiName = samplePois[2].Name,
                GuestId = "web-demo-d4",
                PurchaserDisplayName = "guid-web-demo-d4",
                CustomerLabel = "guest:web-demo-d4",
                Amount = PublicPoiPrice,
                Provider = "ByetHost-Web",
                Status = "Failed",
                CreatedAt = DateTime.Now.AddMinutes(-12)
            }
        };

        if (model.SelectedPoiId.HasValue)
        {
            demoTransactions = demoTransactions
                .Where(item => item.PoiId == model.SelectedPoiId.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(model.SelectedStatus))
        {
            demoTransactions = demoTransactions
                .Where(item => string.Equals(item.Status, model.SelectedStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        demoTransactions = demoTransactions
            .Where(item => item.CreatedAt >= model.StartDate && item.CreatedAt <= model.EndDate)
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        var paidTransactions = demoTransactions
            .Where(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            .ToList();

        model.IsDemoData = true;
        model.DemoMessage = "Day la mock data de demo luong QR Payment trong luc API local chua bat.";
        model.Pois = samplePois;
        model.Transactions = demoTransactions;
        model.TotalTransactions = demoTransactions.Count;
        model.PaidTransactions = paidTransactions.Count;
        model.PendingTransactions = demoTransactions.Count(item => string.Equals(item.Status, "Pending", StringComparison.OrdinalIgnoreCase));
        model.FailedTransactions = demoTransactions.Count(item => string.Equals(item.Status, "Failed", StringComparison.OrdinalIgnoreCase));
        model.ReconciledTransactions = demoTransactions.Count(item => item.ReconciledAt.HasValue);
        model.UnlockedPurchases = paidTransactions.Count;
        model.TotalRevenue = paidTransactions.Sum(item => item.Amount);
        model.AverageOrderValue = paidTransactions.Count == 0 ? 0 : decimal.Round(paidTransactions.Average(item => item.Amount), 0);
        model.TopPois = paidTransactions
            .GroupBy(item => new { item.PoiId, item.PoiName })
            .Select(group => new PaymentPoiStat
            {
                PoiId = group.Key.PoiId,
                PoiName = group.Key.PoiName,
                Revenue = group.Sum(item => item.Amount),
                TransactionCount = group.Count()
            })
            .OrderByDescending(item => item.Revenue)
            .ThenBy(item => item.PoiName)
            .ToList();
        model.ProviderBreakdown = demoTransactions
            .GroupBy(item => item.Provider)
            .Select(group => new PaymentProviderStat
            {
                Provider = group.Key,
                TransactionCount = group.Count(),
                PaidCount = group.Count(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase)),
                Revenue = group.Where(item => string.Equals(item.Status, "Paid", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount)
            })
            .OrderByDescending(item => item.Revenue)
            .ThenBy(item => item.Provider)
            .ToList();
    }

    private sealed class PaymentAccessDto
    {
        public bool HasAccess { get; set; }
    }
}
