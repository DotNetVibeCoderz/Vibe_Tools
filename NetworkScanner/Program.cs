using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Newtonsoft.Json;

namespace NetworkScanner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup Title Keren
            AnsiConsole.Write(
                new FigletText("Jacky NetScan")
                    .Color(Color.Cyan1));
            
            AnsiConsole.MarkupLine("[bold yellow]Network Scanner CLI v1.0 - Cross Platform[/]");
            AnsiConsole.MarkupLine("[grey]Created by Jacky the Code Bender[/]");
            AnsiConsole.WriteLine();

            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What do you want to do?")
                        .PageSize(10)
                        .AddChoices(new[] {
                            "Scan Subnet",
                            "Scan Specific Host Ports",
                            "Wake-on-LAN (WOL)",
                            "Exit"
                        }));

                switch (choice)
                {
                    case "Scan Subnet":
                        await RunSubnetScan();
                        break;
                    case "Scan Specific Host Ports":
                        await RunPortScan();
                        break;
                    case "Wake-on-LAN (WOL)":
                        await RunWOL();
                        break;
                    case "Exit":
                        return;
                }
                
                AnsiConsole.MarkupLine("\nPress [green]Enter[/] to return to menu...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        static async Task RunSubnetScan()
        {
            var subnet = AnsiConsole.Ask<string>("Enter Subnet Base IP (e.g. [green]192.168.1[/]): ");
            
            // Validasi input sederhana
            if (subnet.Split('.').Length != 3)
            {
                AnsiConsole.MarkupLine("[red]Invalid subnet format! Use format like 192.168.1[/]");
                return;
            }

            var scanner = new CoreScanner();
            List<HostInfo> results = new List<HostInfo>();

            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[green]Scanning {subnet}.1-254[/]", maxValue: 254);
                    results = await scanner.ScanSubnetAsync(subnet, task);
                });

            // Tampilkan Hasil
            var table = new Table();
            table.AddColumn("IP Address");
            table.AddColumn("Hostname");
            table.AddColumn("Status");
            table.AddColumn("OS Guess (TTL)");
            table.AddColumn("Latency");

            foreach (var host in results)
            {
                table.AddRow(
                    host.IPAddress ?? "-",
                    host.Hostname ?? "-",
                    host.Status == "Online" ? "[green]Online[/]" : "[red]Offline[/]",
                    host.OSDescription ?? "Unknown",
                    $"{host.RoundTripTime}ms"
                );
            }

            AnsiConsole.Write(table);

            if (results.Any())
            {
                if (AnsiConsole.Confirm("Do you want to scan open ports for these hosts?"))
                {
                    await AnsiConsole.Progress()
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask("[yellow]Scanning Ports...[/]", maxValue: results.Count * CommonPorts.List.Count);
                        foreach (var host in results)
                        {
                            await scanner.ScanPortsAsync(host, task);
                        }
                    });

                    // Tampilkan lagi dengan port
                     var detailedTable = new Table();
                    detailedTable.AddColumn("IP Address");
                    detailedTable.AddColumn("Open Ports");

                    foreach (var host in results)
                    {
                        var ports = host.OpenPorts.Count > 0 
                            ? string.Join(", ", host.OpenPorts.Select(p => $"{p} ({CommonPorts.List.GetValueOrDefault(p, "Unknown")})")) 
                            : "[grey]None[/]";
                        
                        detailedTable.AddRow(host.IPAddress ?? "-", ports);
                    }
                    AnsiConsole.Write(detailedTable);
                }

                // Export Option
                if (AnsiConsole.Confirm("Export results to JSON?"))
                {
                    var json = JsonConvert.SerializeObject(results, Formatting.Indented);
                    var filename = $"scan_result_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    await File.WriteAllTextAsync(filename, json);
                    AnsiConsole.MarkupLine($"[green]Results saved to {filename}[/]");
                }
            }
        }

        static async Task RunPortScan()
        {
            var ip = AnsiConsole.Ask<string>("Enter IP Address to scan: ");
            var scanner = new CoreScanner();
            
            AnsiConsole.MarkupLine($"[yellow]Pinging {ip}...[/]");
            var host = await scanner.ScanHostAsync(ip);

            if (host == null)
            {
                AnsiConsole.MarkupLine("[red]Host unreachable![/]");
                return;
            }

            AnsiConsole.MarkupLine($"[green]Host is Online! Latency: {host.RoundTripTime}ms[/]");
            
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[yellow]Scanning Common Ports...[/]", maxValue: CommonPorts.List.Count);
                    await scanner.ScanPortsAsync(host, task);
                });

            var table = new Table();
            table.AddColumn("Port");
            table.AddColumn("Service");
            table.AddColumn("Status");

            foreach (var port in host.OpenPorts)
            {
                table.AddRow(
                    port.ToString(), 
                    CommonPorts.List.GetValueOrDefault(port, "Unknown"), 
                    "[green]Open[/]"
                );
            }

            if (host.OpenPorts.Count == 0)
            {
                table.AddRow("-", "-", "[red]No common ports open[/]");
            }

            AnsiConsole.Write(table);
        }

        static async Task RunWOL()
        {
            var mac = AnsiConsole.Ask<string>("Enter MAC Address (format: 00-11-22-33-44-55): ");
            try
            {
                await WakeOnLan.SendMagicPacketAsync(mac);
                AnsiConsole.MarkupLine($"[green]Magic Packet sent to {mac}![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
        }
    }
}
