using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace JunkCleaner
{
    public class CleanerEngine
    {
        // P/Invoke for Recycle Bin
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        [Flags]
        enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }

        public event Action<string> OnProgress;

        private void Report(string message)
        {
            OnProgress?.Invoke(message);
        }

        public async Task<long> ScanTempFiles()
        {
            return await Task.Run(() =>
            {
                long size = 0;
                var tempPath = Path.GetTempPath();
                Report($"Scanning Temp: {tempPath}");
                try
                {
                    var dir = new DirectoryInfo(tempPath);
                    foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                    {
                        try { size += file.Length; } catch { }
                    }
                }
                catch { }
                return size;
            });
        }

        public async Task CleanTempFiles()
        {
            await Task.Run(() =>
            {
                var tempPath = Path.GetTempPath();
                Report($"Cleaning Temp: {tempPath}");
                try
                {
                    var dir = new DirectoryInfo(tempPath);
                    foreach (var file in dir.GetFiles())
                    {
                        try 
                        { 
                            file.Delete(); 
                            Report($"Deleted: {file.Name}");
                        } 
                        catch { }
                    }
                    foreach (var subDir in dir.GetDirectories())
                    {
                        try { subDir.Delete(true); } catch { }
                    }
                }
                catch { }
            });
        }

        public async Task CleanRecycleBin()
        {
            await Task.Run(() =>
            {
                Report("Emptying Recycle Bin...");
                try
                {
                    SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND);
                    Report("Recycle Bin Emptied.");
                }
                catch (Exception ex)
                {
                    Report($"Error emptying recycle bin: {ex.Message}");
                }
            });
        }

        public async Task<long> ScanBrowserCache()
        {
            return await Task.Run(() =>
            {
                long size = 0;
                var paths = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\Cache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Mozilla\Firefox\Profiles")
                };

                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        Report($"Scanning Browser Path: {path}");
                        try
                        {
                            var dir = new DirectoryInfo(path);
                            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                            {
                                try { size += file.Length; } catch { }
                            }
                        }
                        catch { }
                    }
                }
                return size;
            });
        }

        public async Task CleanBrowserCache()
        {
            await Task.Run(() =>
            {
                var paths = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Cache"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\Cache")
                };

                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        Report($"Cleaning Browser Path: {path}");
                        try
                        {
                            var dir = new DirectoryInfo(path);
                            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                            {
                                try { file.Delete(); } catch { }
                            }
                        }
                        catch { }
                    }
                }
            });
        }

        public async Task<List<string>> ScanLargeFiles(string path, long minSizeInBytes = 104857600) // Default 100MB
        {
             return await Task.Run(() =>
             {
                 var largeFiles = new List<string>();
                 Report($"Scanning for large files in {path}...");
                 try
                 {
                     var dir = new DirectoryInfo(path);
                     foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories).Take(5000)) // Limit scan for demo performance
                     {
                         try
                         {
                             if (file.Length > minSizeInBytes)
                             {
                                 largeFiles.Add(file.FullName);
                                 Report($"Found Large File: {file.Name} ({file.Length / 1024 / 1024} MB)");
                             }
                         }
                         catch { }
                     }
                 }
                 catch { }
                 return largeFiles;
             });
        }
    }
}