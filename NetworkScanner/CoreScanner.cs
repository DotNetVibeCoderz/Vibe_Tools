using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace NetworkScanner
{
    public class CoreScanner
    {
        // Scan Subnet range (misal 192.168.1.1 - 254)
        public async Task<List<HostInfo>> ScanSubnetAsync(string baseIp, ProgressTask progressTask = null)
        {
            var results = new List<HostInfo>();
            var tasks = new List<Task>();
            
            // Batasi concurrency biar gak crash socketnya
            using (var semaphore = new SemaphoreSlim(50)) 
            {
                for (int i = 1; i < 255; i++)
                {
                    string ip = $"{baseIp}.{i}";
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            var host = await ScanHostAsync(ip);
                            if (host != null)
                            {
                                lock (results)
                                {
                                    results.Add(host);
                                }
                                // Update UI kalau ada progress bar
                                if (progressTask != null)
                                {
                                    progressTask.Description = $"Found: {host.IPAddress}";
                                }
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                            if (progressTask != null) progressTask.Increment(1);
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
            
            // Sort IP address secara logis
            return results.OrderBy(h => Version.Parse(h.IPAddress)).ToList();
        }

        // Scan Host Individu (Ping + DNS + OS Fingerprint dasar)
        public async Task<HostInfo> ScanHostAsync(string ipAddress)
        {
            using (var ping = new Ping())
            {
                try
                {
                    // Ping timeout 500ms biar cepet
                    var reply = await ping.SendPingAsync(ipAddress, 500);
                    
                    if (reply.Status == IPStatus.Success)
                    {
                        var hostInfo = new HostInfo
                        {
                            IPAddress = ipAddress,
                            Status = "Online",
                            RoundTripTime = reply.RoundtripTime,
                            OSDescription = EstimateOS(reply.Options?.Ttl)
                        };

                        // Coba resolve hostname
                        try
                        {
                            var entry = await Dns.GetHostEntryAsync(ipAddress);
                            hostInfo.Hostname = entry.HostName;
                        }
                        catch 
                        {
                            hostInfo.Hostname = "N/A";
                        }

                        // NOTE: Mendapatkan MAC Address remote secara reliable di .NET Cross-Platform 
                        // tanpa library native/admin rights sangat susah. 
                        // Di Windows bisa pakai ARP table (iphlpapi.dll), di Linux baca /proc/net/arp.
                        // Untuk demo ini kita skip MAC scanning yang kompleks agar aman di semua OS.
                        hostInfo.MACAddress = "Requires Admin/Root"; 

                        return hostInfo;
                    }
                }
                catch { }
            }
            return null;
        }

        // Port Scanner
        public async Task ScanPortsAsync(HostInfo host, ProgressTask progress = null)
        {
            var tasks = new List<Task>();
            using (var semaphore = new SemaphoreSlim(20)) // Batasi koneksi port simultan
            {
                foreach (var port in CommonPorts.List.Keys)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            using (var client = new TcpClient())
                            {
                                var connectTask = client.ConnectAsync(host.IPAddress, port);
                                // Timeout 2 detik per port
                                if (await Task.WhenAny(connectTask, Task.Delay(500)) == connectTask)
                                {
                                    if (client.Connected)
                                    {
                                        lock (host.OpenPorts)
                                        {
                                            host.OpenPorts.Add(port);
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        finally
                        {
                            semaphore.Release();
                            if (progress != null) progress.Increment(1);
                        }
                    }));
                }
                await Task.WhenAll(tasks);
            }
            host.OpenPorts.Sort();
        }

        // Menebak OS dari TTL (Time To Live) Ping
        // Ini metode klasik, tidak 100% akurat tapi lumayan untuk perkiraan.
        private string EstimateOS(int? ttl)
        {
            if (!ttl.HasValue) return "Unknown";

            // TTL umum: Windows=128, Linux/Unix=64, Cisco=255
            if (ttl <= 64) return "Linux/Unix/Mac";
            if (ttl <= 128) return "Windows";
            if (ttl <= 255) return "Network Appliance (Cisco/Etc)";
            
            return "Unknown";
        }
    }
}
