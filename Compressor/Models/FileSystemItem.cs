using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Compressor.Models
{
    public partial class FileSystemItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string fullPath = string.Empty;

        [ObservableProperty]
        private bool isDirectory;

        [ObservableProperty]
        private long size;

        [ObservableProperty]
        private DateTime dateModified;

        [ObservableProperty]
        private string type = string.Empty;

        public string SizeDisplay => IsDirectory ? "" : FormatBytes(Size);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }
    }
}