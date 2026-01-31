using System.Collections.Generic;

namespace StockInfo.Models;

public class StockData
{
    public string CompanyProfile { get; set; } = string.Empty;
    public string CurrentPrice { get; set; } = string.Empty;
    public string PriceChange { get; set; } = string.Empty;
    public List<FinancialReport> FinancialReports { get; set; } = new();
    public List<RatioItem> Ratios { get; set; } = new();
    public List<DividendItem> Dividends { get; set; } = new();
    public List<string> Indicators { get; set; } = new();
    public List<string> Trends { get; set; } = new();
    public List<string> Supports { get; set; } = new();
    public List<AlertItem> Alerts { get; set; } = new();
    public List<NewsItem> News { get; set; } = new();
    public List<CalendarItem> Calendar { get; set; } = new();
}
