using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers;

public class PaymentController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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
            model.TotalRevenue = root.GetProperty("totalRevenue").GetDecimal();
            model.AverageOrderValue = root.GetProperty("averageOrderValue").GetDecimal();
            model.TopPois = DeserializeList<PaymentPoiStat>(root.GetProperty("topPois"));
            model.ProviderBreakdown = DeserializeList<PaymentProviderStat>(root.GetProperty("providerBreakdown"));
            model.Transactions = DeserializeList<PaymentTransaction>(root.GetProperty("transactions"));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Khong the tai QR Payment Dashboard vi API chua chay.";
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

    private static List<T> DeserializeList<T>(JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
    }
}
