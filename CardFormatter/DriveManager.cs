using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CardFormatter
{
    public class DriveManager
    {
        public static List<DriveInfo> GetRemovableDrives()
        {
            try
            {
                // Filter for Removable drives to avoid formatting the OS drive by accident!
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .ToList();
                return drives;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching drives: {ex.Message}");
                return new List<DriveInfo>();
            }
        }

        public static void ShowDriveDetails(DriveInfo drive)
        {
            Console.WriteLine($"\n--- Drive Information: {drive.Name} ---");
            Console.WriteLine($"  Volume Label: {drive.VolumeLabel}");
            Console.WriteLine($"  Format:       {drive.DriveFormat}");
            Console.WriteLine($"  Type:         {drive.DriveType}");
            Console.WriteLine($"  Total Size:   {FormatBytes(drive.TotalSize)}");
            Console.WriteLine($"  Free Space:   {FormatBytes(drive.TotalFreeSpace)}");
            Console.WriteLine("------------------------------");
        }

        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }
}
