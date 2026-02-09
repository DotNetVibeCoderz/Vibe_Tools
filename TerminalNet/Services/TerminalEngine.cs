using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TerminalNet.Services
{
    public class TerminalEngine
    {
        public string CurrentDirectory { get; private set; }
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = 0;

        public TerminalEngine()
        {
            CurrentDirectory = Directory.GetCurrentDirectory();
        }

        public void AddToHistory(string cmd)
        {
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                _commandHistory.Add(cmd);
                _historyIndex = _commandHistory.Count;
            }
        }

        public string? GetPreviousHistory()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                return _commandHistory[_historyIndex];
            }
            return null;
        }

        public string? GetNextHistory()
        {
            if (_historyIndex < _commandHistory.Count - 1)
            {
                _historyIndex++;
                return _commandHistory[_historyIndex];
            }
            else
            {
                _historyIndex = _commandHistory.Count;
                return ""; // Kosongkan input
            }
        }

        public async Task<string> ExecuteCommandAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            try
            {
                switch (command)
                {
                    case "help":
                        return GetHelp();
                    case "ls":
                    case "dir":
                        return ListDirectory(args);
                    case "cd":
                        return ChangeDirectory(args);
                    case "pwd":
                        return CurrentDirectory;
                    case "cat":
                        return ReadFile(args);
                    case "echo":
                        return string.Join(" ", args);
                    case "clear":
                        return "[CLEAR]"; // Special token handled by UI
                    case "exit":
                        Environment.Exit(0);
                        return "";
                    case "history":
                        return string.Join(Environment.NewLine, _commandHistory);
                    case "touch":
                        return CreateFile(args);
                    case "rm":
                        return RemoveFile(args);
                    case "mkdir":
                        return CreateDirectory(args);
                    case "rmdir":
                        return RemoveDirectory(args);
                    case "cp":
                        return CopyFile(args);
                    case "mv":
                        return MoveFile(args);
                    case "whoami":
                        return Environment.UserName;
                    case "date":
                        return DateTime.Now.ToString("F");
                    case "ping":
                        return await PingHostAsync(args);
                    case "version":
                        return "TerminalNet v1.0.0 (Avalonia UI) - Created by Jacky Code Bender";
                    case "curl":
                        return await CurlUrlAsync(args);
                    case "ifconfig":
                        return GetNetworkInfo();
                    default:
                        return $"Command '{command}' not found. Type 'help' for list of commands.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string GetHelp()
        {
            return @"Available Commands:
  help           Show this help message
  ls / dir       List files/directories
  cd <path>      Change directory
  pwd            Print working directory
  cat <file>     Read file content
  echo <text>    Display text
  clear          Clear terminal screen
  exit           Close terminal
  history        Show command history
  touch <file>   Create empty file
  rm <file>      Remove file
  mkdir <name>   Create directory
  rmdir <name>   Remove empty directory
  cp <src> <dst> Copy file
  mv <src> <dst> Move/Rename file
  whoami         Show current user
  date           Show current date/time
  ping <host>    Ping a host
  version        Show app version
  curl <url>     Fetch URL content
  ifconfig       Show network configuration";
        }

        private string ListDirectory(string[] args)
        {
            string path = args.Length > 0 ? args[0] : CurrentDirectory;
            if (!Directory.Exists(path)) return $"Directory not found: {path}";
            
            var entries = Directory.GetFileSystemEntries(path);
            return string.Join(Environment.NewLine, entries.Select(e => Path.GetFileName(e)));
        }

        private string ChangeDirectory(string[] args)
        {
            if (args.Length == 0) return CurrentDirectory;
            string newPath = Path.Combine(CurrentDirectory, args[0]);
            string fullPath = Path.GetFullPath(newPath);

            if (Directory.Exists(fullPath))
            {
                CurrentDirectory = fullPath;
                return ""; // Sukses diam-diam
            }
            return $"Directory not found: {args[0]}";
        }

        private string ReadFile(string[] args)
        {
            if (args.Length == 0) return "Usage: cat <filename>";
            string path = Path.Combine(CurrentDirectory, args[0]);
            if (File.Exists(path)) return File.ReadAllText(path);
            return "File not found.";
        }

        private string CreateFile(string[] args)
        {
            if (args.Length == 0) return "Usage: touch <filename>";
            string path = Path.Combine(CurrentDirectory, args[0]);
            File.Create(path).Close();
            return $"File '{args[0]}' created.";
        }

        private string RemoveFile(string[] args)
        {
            if (args.Length == 0) return "Usage: rm <filename>";
            string path = Path.Combine(CurrentDirectory, args[0]);
            if (File.Exists(path))
            {
                File.Delete(path);
                return $"File '{args[0]}' deleted.";
            }
            return "File not found.";
        }

        private string CreateDirectory(string[] args)
        {
            if (args.Length == 0) return "Usage: mkdir <dirname>";
            string path = Path.Combine(CurrentDirectory, args[0]);
            Directory.CreateDirectory(path);
            return $"Directory '{args[0]}' created.";
        }

        private string RemoveDirectory(string[] args)
        {
            if (args.Length == 0) return "Usage: rmdir <dirname>";
            string path = Path.Combine(CurrentDirectory, args[0]);
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
                return $"Directory '{args[0]}' deleted.";
            }
            return "Directory not found.";
        }

        private string CopyFile(string[] args)
        {
            if (args.Length < 2) return "Usage: cp <source> <dest>";
            string src = Path.Combine(CurrentDirectory, args[0]);
            string dst = Path.Combine(CurrentDirectory, args[1]);

            if (File.Exists(src))
            {
                File.Copy(src, dst, true);
                return $"Copied '{args[0]}' to '{args[1]}'.";
            }
            return "Source file not found.";
        }

        private string MoveFile(string[] args)
        {
            if (args.Length < 2) return "Usage: mv <source> <dest>";
            string src = Path.Combine(CurrentDirectory, args[0]);
            string dst = Path.Combine(CurrentDirectory, args[1]);

            if (File.Exists(src))
            {
                File.Move(src, dst);
                return $"Moved '{args[0]}' to '{args[1]}'.";
            }
            else if (Directory.Exists(src))
            {
                Directory.Move(src, dst);
                return $"Moved directory '{args[0]}' to '{args[1]}'.";
            }
            return "Source not found.";
        }

        private async Task<string> PingHostAsync(string[] args)
        {
            if (args.Length == 0) return "Usage: ping <host>";
            try
            {
                using var pinger = new Ping();
                string host = args[0];
                var reply = await pinger.SendPingAsync(host);
                if (reply.Status == IPStatus.Success)
                {
                    return $"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl}";
                }
                return $"Ping failed: {reply.Status}";
            }
            catch (Exception ex)
            {
                return $"Ping error: {ex.Message}";
            }
        }

        private async Task<string> CurlUrlAsync(string[] args)
        {
            if (args.Length == 0) return "Usage: curl <url>";
            try
            {
                using var client = new HttpClient();
                string url = args[0];
                if (!url.StartsWith("http")) url = "http://" + url;
                
                var response = await client.GetStringAsync(url);
                // Batasi output jika terlalu panjang
                if (response.Length > 2000) return response.Substring(0, 2000) + "\n... (truncated)";
                return response;
            }
            catch (Exception ex)
            {
                return $"Request failed: {ex.Message}";
            }
        }

        private string GetNetworkInfo()
        {
            var output = new System.Text.StringBuilder();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    output.AppendLine($"{ni.Name}:");
                    output.AppendLine($"  Status: {ni.OperationalStatus}");
                    output.AppendLine($"  Mac Address: {ni.GetPhysicalAddress()}");
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            output.AppendLine($"  IPv4: {ip.Address}");
                            output.AppendLine($"  Mask: {ip.IPv4Mask}");
                        }
                    }
                    output.AppendLine();
                }
            }
            return output.ToString();
        }
    }
}