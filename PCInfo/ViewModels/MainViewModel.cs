using Avalonia.Threading;
using Hardware.Info;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PCInfo.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private string _osInfo = "Detecting...";
        public string OsInfo
        {
            get => _osInfo;
            set => this.RaiseAndSetIfChanged(ref _osInfo, value);
        }

        private string _cpuInfo = "";
        public string CpuInfo
        {
            get => _cpuInfo;
            set => this.RaiseAndSetIfChanged(ref _cpuInfo, value);
        }

        private string _ramInfo = "";
        public string RamInfo
        {
            get => _ramInfo;
            set => this.RaiseAndSetIfChanged(ref _ramInfo, value);
        }

        private string _gpuInfo = "";
        public string GpuInfo
        {
            get => _gpuInfo;
            set => this.RaiseAndSetIfChanged(ref _gpuInfo, value);
        }
        
        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
             get => _isLoading;
             set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        // Network Specific Properties
        private string _domainInfo = "";
        public string DomainInfo
        {
            get => _domainInfo;
            set => this.RaiseAndSetIfChanged(ref _domainInfo, value);
        }

        private string _internetStatus = "Checking...";
        public string InternetStatus
        {
            get => _internetStatus;
            set => this.RaiseAndSetIfChanged(ref _internetStatus, value);
        }

        public ObservableCollection<HardwareItem> NetworkInterfaces { get; } = new ObservableCollection<HardwareItem>();
        public ObservableCollection<HardwareItem> DriveList { get; } = new ObservableCollection<HardwareItem>();
        
        private readonly IHardwareInfo _hardwareInfo;

        public MainViewModel()
        {
            _hardwareInfo = new HardwareInfo();
            
            // Start scanning in background
            Task.Run(LoadData);
        }

        private async Task LoadData()
        {
            IsLoading = true;
            StatusMessage = "Scanning System... (This might take a moment)";

            await Task.Delay(500); // UI breathing room

            try
            {
                // Refresh Hardware Info
                _hardwareInfo.RefreshAll();

                // 1. Check Internet Connection (Ping Google DNS)
                bool isConnected = false;
                try
                {
                    using (var ping = new Ping())
                    {
                        var reply = await ping.SendPingAsync("8.8.8.8", 2000);
                        isConnected = reply.Status == IPStatus.Success;
                    }
                }
                catch { isConnected = false; }
                
                var globalProps = IPGlobalProperties.GetIPGlobalProperties();
                string domainName = string.IsNullOrEmpty(globalProps.DomainName) ? "Workgroup" : globalProps.DomainName;
                string hostName = globalProps.HostName;

                // Use Dispatcher to update UI from background thread
                Dispatcher.UIThread.Post(() =>
                {
                    // Update Network Global Info
                    InternetStatus = isConnected ? "Online (Connected to Internet)" : "Offline";
                    DomainInfo = $"Host: {hostName}\nDomain: {domainName}";

                    // OS Info
                    OsInfo = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})";
                    
                    // CPU
                    if (_hardwareInfo.CpuList.Count > 0)
                    {
                        var cpu = _hardwareInfo.CpuList[0];
                        CpuInfo = $"{cpu.Name}\n{cpu.NumberOfCores} Cores, {cpu.NumberOfLogicalProcessors} Threads\nMax Clock: {(cpu.MaxClockSpeed / 1000.0):F2} GHz";
                    }
                    else
                    {
                        CpuInfo = "Unknown CPU";
                    }

                    // RAM
                    decimal totalRamBytes = _hardwareInfo.MemoryList.Sum(x => (decimal)x.Capacity);
                    decimal totalRamGb = totalRamBytes / (1024 * 1024 * 1024);
                    RamInfo = $"{totalRamGb:F2} GB Total RAM\n{_hardwareInfo.MemoryList.Count} Sticks Detected";

                    // GPU
                    if (_hardwareInfo.VideoControllerList.Count > 0)
                    {
                        var gpu = _hardwareInfo.VideoControllerList[0];
                        GpuInfo = $"{gpu.Name}\nDriver: {gpu.DriverVersion}\nVRAM: {gpu.AdapterRAM / (1024 * 1024)} MB";
                    }
                    else
                    {
                        GpuInfo = "No GPU Detected";
                    }

                    // Drives
                    DriveList.Clear();
                    foreach (var drive in _hardwareInfo.DriveList)
                    {
                         DriveList.Add(new HardwareItem { 
                             Name = drive.Model, 
                             Description = $"Size: {drive.Size / (1024.0 * 1024 * 1024):F2} GB\nPartitions: {drive.Partitions}" 
                         });
                    }

                    // Network Interfaces (Detailed)
                    NetworkInterfaces.Clear();
                    var nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var adapter in nics)
                    {
                        // Skip Loopback and inactive (optional, but cleaner)
                        if(adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                        var properties = adapter.GetIPProperties();
                        
                        // Get IP and Subnet
                        var ipv4 = properties.UnicastAddresses
                            .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork);
                        
                        string ipAddress = ipv4?.Address.ToString() ?? "N/A";
                        string subnet = ipv4?.IPv4Mask.ToString() ?? "N/A";

                        // Get DNS
                        var dnsAddresses = properties.DnsAddresses
                            .Where(d => d.AddressFamily == AddressFamily.InterNetwork)
                            .Select(d => d.ToString());
                        string dns = dnsAddresses.Any() ? string.Join(", ", dnsAddresses) : "Automatic/None";

                        // Connection Type
                        string connType = adapter.NetworkInterfaceType.ToString();
                        
                        // Formatting Description
                        string details = $"Type: {connType}\n" +
                                         $"Status: {adapter.OperationalStatus}\n" +
                                         $"IP Address: {ipAddress}\n" +
                                         $"Subnet Mask: {subnet}\n" +
                                         $"DNS: {dns}\n" +
                                         $"MAC: {adapter.GetPhysicalAddress()}";

                        NetworkInterfaces.Add(new HardwareItem {
                            Name = adapter.Name, // e.g., Ethernet, Wi-Fi
                            Description = details
                        });
                    }

                    StatusMessage = "Scan Complete.";
                    IsLoading = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    StatusMessage = $"Error: {ex.Message}";
                    IsLoading = false;
                });
            }
        }
    }

    public class HardwareItem
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}