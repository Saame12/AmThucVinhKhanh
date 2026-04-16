using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using VinhKhanhFood.Admin.Models;

namespace VinhKhanhFood.Admin.Controllers;

public class UsageController : Controller
{
    private readonly HttpClient _http;

    public UsageController()
    {
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5020/api/") };
    }

    public async Task<IActionResult> Index(string period = "week")
    {
        try
        {
            var data = await _http.GetFromJsonAsync<List<UsageHistory>>("Food/history") ?? new List<UsageHistory>();
            var model = BuildDashboard(data, period);
            return View(model);
        }
        catch
        {
            TempData["Error"] = "Khong the tai lich su su dung vi API chua chay.";
            return View(new UsageDashboardViewModel
            {
                SelectedPeriod = NormalizePeriod(period),
                PeriodLabel = GetPeriodLabel(NormalizePeriod(period))
            });
        }
    }

    private static UsageDashboardViewModel BuildDashboard(List<UsageHistory> allHistory, string period)
    {
        var normalizedPeriod = NormalizePeriod(period);
        var periodRange = ResolvePeriodRange(normalizedPeriod);

        var travelerHistory = allHistory
            .Where(item => string.Equals(item.Action, "VIEW_DETAIL", StringComparison.OrdinalIgnoreCase))
            .Where(item => !string.Equals(item.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            .Where(item => !string.Equals(item.Role, "Owner", StringComparison.OrdinalIgnoreCase))
            .Where(item => item.CreatedAt >= periodRange.Start && item.CreatedAt < periodRange.End)
            .OrderByDescending(item => item.CreatedAt)
            .ToList();

        var rankings = travelerHistory
            .GroupBy(item => new { item.PoiId, PoiName = item.PoiName ?? $"POI {item.PoiId}" })
            .Select(group => new PoiUsageStat
            {
                PoiId = group.Key.PoiId,
                PoiName = group.Key.PoiName,
                VisitCount = group.Count()
            })
            .OrderByDescending(item => item.VisitCount)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var leastVisited = rankings
            .OrderBy(item => item.VisitCount)
            .ThenBy(item => item.PoiName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        return new UsageDashboardViewModel
        {
            SelectedPeriod = normalizedPeriod,
            PeriodLabel = GetPeriodLabel(normalizedPeriod),
            TotalTravelerViews = travelerHistory.Count,
            DistinctPois = rankings.Count,
            MostVisitedPoi = rankings.FirstOrDefault(),
            LeastVisitedPoi = leastVisited,
            Rankings = rankings,
            HistoryItems = travelerHistory.Take(100).ToList()
        };
    }

    private static string NormalizePeriod(string? period) => period?.Trim().ToLowerInvariant() switch
    {
        "month" => "month",
        "year" => "year",
        _ => "week"
    };

    private static (DateTime Start, DateTime End) ResolvePeriodRange(string period)
    {
        var now = DateTime.Now;
        return period switch
        {
            "month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
            "year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1)),
            _ => ResolveWeekRange(now)
        };
    }

    private static (DateTime Start, DateTime End) ResolveWeekRange(DateTime now)
    {
        var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var start = now.Date.AddDays(-diff);
        return (start, start.AddDays(7));
    }

    private static string GetPeriodLabel(string period) => period switch
    {
        "month" => "Thang nay",
        "year" => "Nam nay",
        _ => "Tuan nay"
    };
}
