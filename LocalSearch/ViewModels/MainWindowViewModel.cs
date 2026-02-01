using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalSearch.Models;
using LocalSearch.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace LocalSearch.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly SearchService _searchService;

        [ObservableProperty]
        private string _rootPath = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready.";

        [ObservableProperty]
        private bool _isSemantic = false;

        [ObservableProperty]
        private bool _isBusy = false;

        public ObservableCollection<SearchResultItem> SearchResults { get; } = new();

        public MainWindowViewModel()
        {
            _searchService = new SearchService();
            // Default path for demo
            RootPath = AppDomain.CurrentDomain.BaseDirectory; 
        }

        [RelayCommand]
        public async Task Index()
        {
            if (string.IsNullOrWhiteSpace(RootPath)) return;

            IsBusy = true;
            StatusMessage = "Indexing started...";
            SearchResults.Clear();

            var progress = new Progress<string>(msg => StatusMessage = msg);

            try
            {
                int count = await _searchService.IndexDirectoryAsync(RootPath, progress);
                StatusMessage = $"Indexing complete. {count} files indexed.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            SearchResults.Clear();
            StatusMessage = "Searching...";

            try
            {
                var results = _searchService.Search(SearchQuery, IsSemantic);
                foreach (var item in results)
                {
                    SearchResults.Add(item);
                }
                StatusMessage = $"Found {SearchResults.Count} results.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search Error: {ex.Message}";
            }
        }

        [RelayCommand]
        public void OpenResult(SearchResultItem item)
        {
            if (item == null) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot open file: {ex.Message}";
            }
        }
    }
}