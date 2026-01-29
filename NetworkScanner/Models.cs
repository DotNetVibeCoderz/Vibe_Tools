using System.Collections.Generic;

namespace NetworkScanner
{
    // Class untuk menyimpan informasi Host yang ditemukan
    public class HostInfo
    {
        public string IPAddress { get; set; }
        public string Hostname { get; set; }
        public string MACAddress { get; set; }
        public string Vendor { get; set; } // Placeholder untuk MAC Vendor lookup
        public string Status { get; set; }
        public string OSDescription { get; set; } // Hasil fingerprinting sederhana
        public long RoundTripTime { get; set; }
        public List<int> OpenPorts { get; set; } = new List<int>();

        public HostInfo()
        {
            OpenPorts = new List<int>();
        }
    }

    // List Port umum untuk discan
    public static class CommonPorts
    {
        public static readonly Dictionary<int, string> List = new Dictionary<int, string>
        {
            { 20, "FTP Data" },
            { 21, "FTP Control" },
            { 22, "SSH" },
            { 23, "Telnet" },
            { 25, "SMTP" },
            { 53, "DNS" },
            { 80, "HTTP" },
            { 110, "POP3" },
            { 135, "RPC" },
            { 139, "NetBIOS" },
            { 143, "IMAP" },
            { 443, "HTTPS" },
            { 445, "SMB" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 5432, "PostgreSQL" },
            { 8080, "HTTP Proxy" }
        };
    }
}
