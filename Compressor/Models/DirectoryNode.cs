using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Compressor.Models
{
    public partial class DirectoryNode : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string fullPath;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value)
                {
                    // Dispatch to ensure UI remains responsive if slightly heavy
                    Dispatcher.UIThread.Post(LoadChildren);
                }
            }
        }

        public ObservableCollection<DirectoryNode> SubFolders { get; } = new ObservableCollection<DirectoryNode>();

        public DirectoryNode(string path)
        {
            FullPath = path;
            Name = path; 
            
            if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);
                // If it's a root drive, keep the name as is (e.g., "C:\"), otherwise use folder name
                if (info.Parent != null) Name = info.Name;
            }

            // Check for subdirectories to show expander
            try
            {
                // Enumerate just one to see if children exist
                if (Directory.EnumerateDirectories(path).Any())
                {
                    SubFolders.Add(new DirectoryNode("Loading...", true));
                }
            }
            catch 
            {
                // Access denied or other errors, ignore
            }
        }

        private DirectoryNode(string name, bool isDummy)
        {
            Name = name;
            FullPath = "";
        }

        private void LoadChildren()
        {
            // Only load if currently has dummy item
            if (SubFolders.Count == 1 && SubFolders[0].Name == "Loading...")
            {
                SubFolders.Clear();
                try
                {
                    var dirs = Directory.GetDirectories(FullPath);
                    foreach (var dir in dirs)
                    {
                        var info = new DirectoryInfo(dir);
                        // Skip hidden folders
                        if ((info.Attributes & FileAttributes.Hidden) == 0)
                        {
                            SubFolders.Add(new DirectoryNode(dir));
                        }
                    }
                }
                catch 
                {
                    // Failed to list directories (permission)
                }
            }
        }
    }
}