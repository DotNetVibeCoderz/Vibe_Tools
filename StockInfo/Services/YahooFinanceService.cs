using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using StockInfo.Models;

namespace StockInfo.Services;

public class YahooFinanceService
{
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("https://query1.finance.yahoo.com/")
    };

    public async Task<StockData> GetStockDataAsync(string symbol)
    {
        var data = new StockData();

        var quoteUrl = $"v10/finance/quoteSummary/{symbol}?modules=price,summaryProfile,summaryDetail,financialData,calendarEvents,earnings";
        var quoteJson = await HttpClient.GetStringAsync(quoteUrl);
        using var quoteDoc = JsonDocument.Parse(quoteJson);

        var result = quoteDoc.RootElement
            .GetProperty("quoteSummary")
            .GetProperty("result")[0];

        var price = TryGetElement(result, "price");
        var summaryProfile = TryGetElement(result, "summaryProfile");
        var summaryDetail = TryGetElement(result, "summaryDetail");
        var financialData = TryGetElement(result, "financialData");
        var calendarEvents = TryGetElement(result, "calendarEvents");
        var earnings = TryGetElement(result, "earnings");

        var companyName = GetString(price, "longName");
        var sector = GetString(summaryProfile, "sector");
        var industry = GetString(summaryProfile, "industry");
        var description = GetString(summaryProfile, "longBusinessSummary");
        var website = GetString(summaryProfile, "website");

        data.CompanyProfile = $"{companyName} (Sektor: {sector}, Industri: {industry}). {description} Situs: {website}";

        var currentPrice = GetFmt(price, "regularMarketPrice");
        var changePercent = GetFmt(price, "regularMarketChangePercent");
        data.CurrentPrice = currentPrice;
        data.PriceChange = changePercent;

        // Fundamental - laporan keuangan
        if (earnings.ValueKind != JsonValueKind.Undefined
            && earnings.TryGetProperty("financialsChart", out var financialsChart)
            && financialsChart.TryGetProperty("yearly", out var yearlyArray))
        {
            foreach (var item in yearlyArray.EnumerateArray())
            {
                data.FinancialReports.Add(new FinancialReport
                {
                    Period = item.TryGetProperty("date", out var date) ? date.GetString() ?? "-" : "-",
                    Revenue = GetMoney(item, "revenue"),
                    NetProfit = GetMoney(item, "earnings"),
                    EPS = GetFmt(financialData, "epsTrailingTwelveMonths")
                });
            }
        }

        // Rasio
        data.Ratios.Add(new RatioItem { Name = "PER", Value = GetFmt(summaryDetail, "trailingPE"), Note = "Valuasi" });
        data.Ratios.Add(new RatioItem { Name = "ROE", Value = GetFmt(financialData, "returnOnEquity"), Note = "Profitabilitas" });
        data.Ratios.Add(new RatioItem { Name = "DER", Value = GetFmt(financialData, "debtToEquity"), Note = "Leverage" });
        data.Ratios.Add(new RatioItem { Name = "PBV", Value = GetFmt(summaryDetail, "priceToBook"), Note = "Nilai buku" });

        // Dividen
        data.Dividends.Add(new DividendItem
        {
            Year = DateTime.UtcNow.Year.ToString(),
            Amount = GetFmt(summaryDetail, "dividendRate"),
            Yield = GetFmt(summaryDetail, "dividendYield")
        });

        // Teknikal
        data.Indicators.Add($"MA50: {GetFmt(summaryDetail, "fiftyDayAverage")}");
        data.Indicators.Add($"MA200: {GetFmt(summaryDetail, "twoHundredDayAverage")}");
        data.Indicators.Add($"52W High: {GetFmt(summaryDetail, "fiftyTwoWeekHigh")}");

        data.Trends.Add($"Pergerakan harian: {GetFmt(price, "regularMarketChange")}");
        data.Trends.Add($"Volume harian: {GetFmt(summaryDetail, "regularMarketVolume")}");

        data.Supports.Add($"Support (52W Low): {GetFmt(summaryDetail, "fiftyTwoWeekLow")}");
        data.Supports.Add($"Resistance (52W High): {GetFmt(summaryDetail, "fiftyTwoWeekHigh")}");

        var priceValue = GetRawDecimal(price, "regularMarketPrice");
        var resistance = GetRawDecimal(summaryDetail, "fiftyTwoWeekHigh");
        var support = GetRawDecimal(summaryDetail, "fiftyTwoWeekLow");
        if (priceValue.HasValue && resistance.HasValue)
        {
            data.Alerts.Add(new AlertItem { Condition = $"Jika harga > {resistance.Value:F0}", Status = "Pantau" });
        }
        if (priceValue.HasValue && support.HasValue)
        {
            data.Alerts.Add(new AlertItem { Condition = $"Jika harga < {support.Value:F0}", Status = "Siaga" });
        }

        // Kalender - tanggal earnings
        if (calendarEvents.ValueKind != JsonValueKind.Undefined
            && calendarEvents.TryGetProperty("earnings", out var earningsEvent)
            && earningsEvent.TryGetProperty("earningsDate", out var earningsDate))
        {
            foreach (var date in earningsDate.EnumerateArray())
            {
                data.Calendar.Add(new CalendarItem
                {
                    Date = date.TryGetProperty("fmt", out var fmt) ? fmt.GetString() ?? "-" : "-",
                    Event = "Rilis laporan keuangan",
                    Impact = "High"
                });
            }
        }

        // News
        await FillNewsAsync(symbol, data);

        return data;
    }

    private static async Task FillNewsAsync(string symbol, StockData data)
    {
        var searchJson = await HttpClient.GetStringAsync($"v1/finance/search?q={symbol}");
        using var searchDoc = JsonDocument.Parse(searchJson);

        if (!searchDoc.RootElement.TryGetProperty("news", out var newsArray)) return;

        foreach (var item in newsArray.EnumerateArray().Take(5))
        {
            var title = item.TryGetProperty("title", out var titleEl) ? titleEl.GetString() ?? "-" : "-";
            var publisher = item.TryGetProperty("publisher", out var pubEl) ? pubEl.GetString() ?? "-" : "-";
            var time = item.TryGetProperty("providerPublishTime", out var timeEl) && timeEl.TryGetInt64(out var unix)
                ? DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString("dd MMM yyyy HH:mm")
                : "-";

            data.News.Add(new NewsItem
            {
                Title = title,
                Source = publisher,
                Time = time
            });
        }
    }

    private static JsonElement TryGetElement(JsonElement parent, string name)
    {
        return parent.ValueKind != JsonValueKind.Undefined && parent.TryGetProperty(name, out var value)
            ? value
            : default;
    }

    private static string GetString(JsonElement parent, string name)
    {
        return parent.ValueKind != JsonValueKind.Undefined && parent.TryGetProperty(name, out var value)
            ? value.GetString() ?? "-"
            : "-";
    }

    private static string GetFmt(JsonElement parent, string name)
    {
        if (parent.ValueKind == JsonValueKind.Undefined) return "-";

        if (parent.TryGetProperty(name, out var value))
        {
            if (value.TryGetProperty("fmt", out var fmt))
            {
                return fmt.GetString() ?? "-";
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDecimal().ToString(CultureInfo.InvariantCulture);
            }
        }

        return "-";
    }

    private static string GetMoney(JsonElement parent, string name)
    {
        if (parent.ValueKind == JsonValueKind.Undefined) return "-";

        if (parent.TryGetProperty(name, out var value))
        {
            if (value.TryGetProperty("fmt", out var fmt))
            {
                return fmt.GetString() ?? "-";
            }

            if (value.TryGetProperty("raw", out var raw) && raw.TryGetDecimal(out var rawValue))
            {
                return rawValue.ToString("N0", CultureInfo.InvariantCulture);
            }
        }

        return "-";
    }

    private static decimal? GetRawDecimal(JsonElement parent, string name)
    {
        if (parent.ValueKind == JsonValueKind.Undefined) return null;

        if (parent.TryGetProperty(name, out var value))
        {
            if (value.TryGetProperty("raw", out var raw) && raw.TryGetDecimal(out var result))
            {
                return result;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var direct))
            {
                return direct;
            }
        }

        return null;
    }
}
