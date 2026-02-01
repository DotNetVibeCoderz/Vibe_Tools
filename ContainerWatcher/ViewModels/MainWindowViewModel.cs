using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Docker.DotNet.Models;

namespace ContainerWatcher.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ContainerService _containerService;

        [ObservableProperty]
        private ObservableCollection<ContainerViewModel> _containers;

        [ObservableProperty]
        private ContainerViewModel? _selectedContainer;

        [ObservableProperty]
        private string _logsText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage;

        public MainWindowViewModel()
        {
            _containerService = new ContainerService();
            _containers = new ObservableCollection<ContainerViewModel>();
            _logsText = "Select a container to view logs...";
            _statusMessage = "Ready";
            
            // Initial Load
            LoadContainersCommand.Execute(null);
        }

        [RelayCommand]
        public async Task LoadContainers()
        {
            IsLoading = true;
            StatusMessage = "Refreshing containers...";
            try
            {
                var list = await _containerService.ListContainersAsync();
                Containers.Clear();
                foreach (var item in list)
                {
                    Containers.Add(new ContainerViewModel(item));
                }
                StatusMessage = $"Loaded {Containers.Count} containers.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task StartSelected()
        {
            if (SelectedContainer == null) return;
            StatusMessage = $"Starting {SelectedContainer.Name}...";
            bool success = await _containerService.StartContainerAsync(SelectedContainer.Id);
            if (success)
            {
                StatusMessage = $"Started {SelectedContainer.Name}";
                await LoadContainers();
            }
            else
            {
                StatusMessage = $"Failed to start {SelectedContainer.Name}";
            }
        }

        [RelayCommand]
        public async Task StopSelected()
        {
            if (SelectedContainer == null) return;
            StatusMessage = $"Stopping {SelectedContainer.Name}...";
            bool success = await _containerService.StopContainerAsync(SelectedContainer.Id);
            if (success)
            {
                StatusMessage = $"Stopped {SelectedContainer.Name}";
                await LoadContainers();
            }
            else
            {
                StatusMessage = $"Failed to stop {SelectedContainer.Name}";
            }
        }

        [RelayCommand]
        public async Task ViewLogs()
        {
            if (SelectedContainer == null) return;
            StatusMessage = $"Fetching logs for {SelectedContainer.Name}...";
            LogsText = "Loading logs...";
            var logs = await _containerService.GetContainerLogsAsync(SelectedContainer.Id);
            LogsText = logs;
            StatusMessage = "Logs loaded.";
        }
        
        partial void OnSelectedContainerChanged(ContainerViewModel? value)
        {
            if(value != null)
            {
               // Auto load logs when selected? maybe not to save resource
               LogsText = "Click 'View Logs' to see logs.";
            }
        }
    }

    public class ContainerViewModel : ObservableObject
    {
        public string Id { get; }
        public string Name { get; }
        public string Image { get; }
        public string State { get; }
        public string Status { get; }
        public string ShortId { get; }

        public bool IsRunning => State?.ToLower() == "running";

        // Helper for UI Color
        public string StatusColor => IsRunning ? "#2ecc71" : "#e74c3c"; // Green : Red

        public ContainerViewModel(ContainerListResponse c)
        {
            Id = c.ID;
            ShortId = c.ID.Length > 10 ? c.ID.Substring(0, 10) : c.ID;
            Name = c.Names?.FirstOrDefault() ?? "Unknown";
            // Remove leading slash if exists
            if (Name.StartsWith("/")) Name = Name.Substring(1);
            
            Image = c.Image;
            State = c.State;
            Status = c.Status;
        }
    }
}