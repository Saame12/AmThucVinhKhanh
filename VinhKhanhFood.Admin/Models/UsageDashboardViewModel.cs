namespace VinhKhanhFood.Admin.Models;

public sealed class UsageDashboardViewModel
{
    public string SelectedPeriod { get; set; } = "week";
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalTravelerViews { get; set; }
    public int DistinctPois { get; set; }
    public PoiUsageStat? MostVisitedPoi { get; set; }
    public PoiUsageStat? LeastVisitedPoi { get; set; }
    public List<PoiUsageStat> Rankings { get; set; } = [];
    public List<UsageHistory> HistoryItems { get; set; } = [];
}

public sealed class PoiUsageStat
{
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
}
