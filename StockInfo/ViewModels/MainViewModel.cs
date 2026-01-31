using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using StockInfo.Models;
using StockInfo.Services;

namespace StockInfo.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly List<StockItem> _allStocks;
    private readonly YahooFinanceService _yahooService;

    private string _stockQuery = string.Empty;
    private string _inputCode = string.Empty;
    private StockItem? _selectedStock;
    private string _companyProfile = string.Empty;
    private string _currentPrice = string.Empty;
    private string _priceChange = string.Empty;

    public MainViewModel()
    {
        _yahooService = new YahooFinanceService();
        _allStocks = new List<StockItem>
        {
            new() { Code = "BBCA", Name = "Bank Central Asia", Sector = "Perbankan" },
            new() { Code = "BBRI", Name = "Bank Rakyat Indonesia", Sector = "Perbankan" },
            new() { Code = "TLKM", Name = "Telkom Indonesia", Sector = "Telekomunikasi" },
            new() { Code = "ASII", Name = "Astra International", Sector = "Otomotif" },
            new() { Code = "UNVR", Name = "Unilever Indonesia", Sector = "Barang Konsumen" }
        };

        FilteredStocks = new ObservableCollection<StockItem>(_allStocks);
        FinancialReports = new ObservableCollection<FinancialReport>();
        Ratios = new ObservableCollection<RatioItem>();
        Dividends = new ObservableCollection<DividendItem>();
        Indicators = new ObservableCollection<string>();
        Trends = new ObservableCollection<string>();
        Supports = new ObservableCollection<string>();
        Alerts = new ObservableCollection<AlertItem>();
        News = new ObservableCollection<NewsItem>();
        Calendar = new ObservableCollection<CalendarItem>();
        Simulations = new ObservableCollection<SimulationResult>();

        SelectedStock = _allStocks.First();
    }

    public ObservableCollection<StockItem> FilteredStocks { get; }
    public ObservableCollection<FinancialReport> FinancialReports { get; }
    public ObservableCollection<RatioItem> Ratios { get; }
    public ObservableCollection<DividendItem> Dividends { get; }
    public ObservableCollection<string> Indicators { get; }
    public ObservableCollection<string> Trends { get; }
    public ObservableCollection<string> Supports { get; }
    public ObservableCollection<AlertItem> Alerts { get; }
    public ObservableCollection<NewsItem> News { get; }
    public ObservableCollection<CalendarItem> Calendar { get; }
    public ObservableCollection<SimulationResult> Simulations { get; }

    public string StockQuery
    {
        get => _stockQuery;
        set
        {
            if (_stockQuery == value) return;
            _stockQuery = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public string InputCode
    {
        get => _inputCode;
        set
        {
            if (_inputCode == value) return;
            _inputCode = value;
            OnPropertyChanged();
        }
    }

    public StockItem? SelectedStock
    {
        get => _selectedStock;
        set
        {
            if (_selectedStock == value) return;
            _selectedStock = value;
            OnPropertyChanged();
            _ = LoadStockDetailsAsync();
        }
    }

    public string CompanyProfile
    {
        get => _companyProfile;
        private set
        {
            if (_companyProfile == value) return;
            _companyProfile = value;
            OnPropertyChanged();
        }
    }

    public string CurrentPrice
    {
        get => _currentPrice;
        private set
        {
            if (_currentPrice == value) return;
            _currentPrice = value;
            OnPropertyChanged();
        }
    }

    public string PriceChange
    {
        get => _priceChange;
        private set
        {
            if (_priceChange == value) return;
            _priceChange = value;
            OnPropertyChanged();
        }
    }

    public void AddOrSelectStock()
    {
        if (string.IsNullOrWhiteSpace(InputCode)) return;

        var code = InputCode.Trim().ToUpperInvariant();
        var existing = _allStocks.FirstOrDefault(s => s.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new StockItem
            {
                Code = code,
                Name = $"Perusahaan {code}",
                Sector = "IDX"
            };
            _allStocks.Add(existing);
        }

        ApplyFilter();
        SelectedStock = existing;
        InputCode = string.Empty;
    }

    private void ApplyFilter()
    {
        var keyword = StockQuery.Trim().ToLowerInvariant();
        var filtered = string.IsNullOrWhiteSpace(keyword)
            ? _allStocks
            : _allStocks.Where(s => s.Code.ToLowerInvariant().Contains(keyword)
                                 || s.Name.ToLowerInvariant().Contains(keyword)
                                 || s.Sector.ToLowerInvariant().Contains(keyword))
                        .ToList();

        FilteredStocks.Clear();
        foreach (var stock in filtered)
        {
            FilteredStocks.Add(stock);
        }
    }

    private async Task LoadStockDetailsAsync()
    {
        if (SelectedStock is null) return;

        CompanyProfile = "Memuat data dari Yahoo Finance...";
        CurrentPrice = "-";
        PriceChange = "-";

        ClearCollections();

        try
        {
            var symbol = BuildYahooSymbol(SelectedStock.Code);
            var data = await _yahooService.GetStockDataAsync(symbol);

            CompanyProfile = data.CompanyProfile;
            CurrentPrice = data.CurrentPrice;
            PriceChange = data.PriceChange;

            UpdateCollection(FinancialReports, data.FinancialReports);
            UpdateCollection(Ratios, data.Ratios);
            UpdateCollection(Dividends, data.Dividends);
            UpdateCollection(Indicators, data.Indicators);
            UpdateCollection(Trends, data.Trends);
            UpdateCollection(Supports, data.Supports);
            UpdateCollection(Alerts, data.Alerts);
            UpdateCollection(News, data.News);
            UpdateCollection(Calendar, data.Calendar);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error loading stock data: {ex.Message}");
            CompanyProfile = "Gagal mengambil data Yahoo Finance. Menampilkan data contoh.";
            LoadFallbackData();
        }

        LoadSimulationDummy();
    }

    private string BuildYahooSymbol(string code)
    {
        return code.EndsWith(".JK", StringComparison.OrdinalIgnoreCase)
            ? code
            : $"{code}.JK";
    }

    private void ClearCollections()
    {
        FinancialReports.Clear();
        Ratios.Clear();
        Dividends.Clear();
        Indicators.Clear();
        Trends.Clear();
        Supports.Clear();
        Alerts.Clear();
        News.Clear();
        Calendar.Clear();
    }

    private void UpdateCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void LoadFallbackData()
    {
        FinancialReports.Add(new FinancialReport { Period = "Q1 2024", Revenue = "Rp 25,3T", NetProfit = "Rp 6,1T", EPS = "Rp 210" });
        FinancialReports.Add(new FinancialReport { Period = "Q4 2023", Revenue = "Rp 23,7T", NetProfit = "Rp 5,4T", EPS = "Rp 190" });

        Ratios.Add(new RatioItem { Name = "PER", Value = "18.2x", Note = "Valuasi sehat" });
        Ratios.Add(new RatioItem { Name = "ROE", Value = "19.5%", Note = "Profitabilitas tinggi" });

        Dividends.Add(new DividendItem { Year = "2024", Amount = "Rp 170", Yield = "2.1%" });

        Indicators.Add("RSI 62 (Bullish moderat)");
        Indicators.Add("MACD Golden Cross");

        Trends.Add("Trend naik sejak 3 bulan terakhir");

        Supports.Add("Support: Rp 8.200");
        Supports.Add("Resistance: Rp 9.100");

        Alerts.Add(new AlertItem { Condition = "Jika harga > Rp 9.000", Status = "Aktif" });

        News.Add(new NewsItem { Title = "Ekspansi digital memperkuat kinerja", Source = "MarketNews", Time = "5 menit lalu" });

        Calendar.Add(new CalendarItem { Date = "20 Jun 2024", Event = "Rilis Laporan Q2", Impact = "High" });
    }

    private void LoadSimulationDummy()
    {
        Simulations.Clear();
        Simulations.Add(new SimulationResult { Scenario = "Bullish", Outcome = "Potensi Rp 9.800", Probability = "45%" });
        Simulations.Add(new SimulationResult { Scenario = "Neutral", Outcome = "Konsolidasi Rp 8.400-8.900", Probability = "35%" });
        Simulations.Add(new SimulationResult { Scenario = "Bearish", Outcome = "Turun ke Rp 7.900", Probability = "20%" });
    }
}
