using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CardFormatter
{
    public static class Formatter
    {
        public enum FileSystem
        {
            FAT32,
            NTFS,
            ExFAT
        }

        // ... Previous code ...

        public static void CheckDisk(string driveLetter)
        {
             Console.WriteLine($"\n[INFO] Starting Error Check on {driveLetter}...");

             if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
             {
                 string drive = driveLetter.Replace("\\", "");
                 // chkdsk E: /F /R (Might need reboot if locked, but for removable it's usually fine)
                 // /F fixes errors.
                 string args = $"/c echo y | chkdsk {drive} /F"; 
                 ExecuteProcess("cmd.exe", args);
             }
             else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
             {
                 // fsck -a /dev/sdX
                 Console.WriteLine("[WARNING] Linux check requires unmounted drive usually.");
                 ExecuteProcess("fsck", $"-a {driveLetter}");
             }
             else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
             {
                 ExecuteProcess("diskutil", $"repairVolume {driveLetter}");
             }
        }

        public static void FormatDrive(string driveLetter, FileSystem fs, string label, bool quickFormat)
        {
            Console.WriteLine($"\n[INFO] Starting format on {driveLetter} ({fs})...");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FormatWindows(driveLetter, fs, label, quickFormat);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("[WARNING] Linux formatting requires root privileges and exact device path (e.g., /dev/sdb1).");
                Console.WriteLine("Please ensure you are running as sudo.");
                FormatLinux(driveLetter, fs, label);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine("[WARNING] MacOS formatting uses diskutil.");
                FormatMac(driveLetter, fs, label);
            }
            else
            {
                Console.WriteLine("[ERROR] Unsupported Operating System.");
            }
        }

        private static void FormatWindows(string driveName, FileSystem fs, string label, bool quick)
        {
            string drive = driveName.Replace("\\", "");
            string fsStr = fs.ToString();
            string quickFlag = quick ? "/Q" : "";
            string args = $"/c format {drive} /FS:{fsStr} /V:{label} {quickFlag} /Y";
            ExecuteProcess("cmd.exe", args);
        }

        private static void FormatLinux(string devicePath, FileSystem fs, string label)
        {
            string tool = "mkfs.vfat"; 
            if (fs == FileSystem.NTFS) tool = "mkfs.ntfs";
            if (fs == FileSystem.ExFAT) tool = "mkfs.exfat";

            string args = $"-n {label} {devicePath}"; 
            
            if (!devicePath.StartsWith("/dev/"))
            {
                Console.WriteLine("[ERROR] Invalid device path for Linux. Must start with /dev/");
                return;
            }

            ExecuteProcess(tool, args);
        }

        private static void FormatMac(string devicePath, FileSystem fs, string label)
        {
            string fsStr = "MS-DOS"; 
            if (fs == FileSystem.ExFAT) fsStr = "ExFAT";
            if (fs == FileSystem.NTFS) { Console.WriteLine("MacOS cannot natively write NTFS formats easily without drivers."); return; }

            string args = $"eraseVolume \"{fsStr}\" \"{label}\" {devicePath}";
            ExecuteProcess("diskutil", args);
        }

        private static void ExecuteProcess(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process == null) return;

                    Console.WriteLine($"[EXECUTING] {fileName} {arguments}");
                    
                    process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"[CMD] {e.Data}"); };
                    process.ErrorDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine($"[ERR] {e.Data}"); };

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                    Console.WriteLine($"[INFO] Process exited with code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] Failed to execute format command: {ex.Message}");
                Console.WriteLine("Ensure you have administrative privileges.");
            }
        }
    }
}
