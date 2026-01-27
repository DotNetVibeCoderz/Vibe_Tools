using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ImageExplorer.Models
{
    public class DirectoryNode
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public ObservableCollection<DirectoryNode> Children { get; } = new ObservableCollection<DirectoryNode>();

        public DirectoryNode(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(Name)) Name = path; 
        }

        public void LoadChildren(bool force = false)
        {
            if (Children.Count > 0 && !force) return;

            try
            {
                Children.Clear();
                var info = new DirectoryInfo(Path);
                foreach (var dir in info.GetDirectories())
                {
                    // Only add if not hidden/system to avoid crashes
                    if ((dir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        Children.Add(new DirectoryNode(dir.FullName));
                    }
                }
            }
            catch { }
        }
    }
}