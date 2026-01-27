using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compressor.Models;
using Compressor.Views;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Compressor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DirectoryNode> roots;

        [ObservableProperty]
        private DirectoryNode? selectedFolder;

        [ObservableProperty]
        private ObservableCollection<FileSystemItem> currentItems;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PasteItemCommand))]
        [NotifyCanExecuteChangedFor(nameof(CutItemCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyItemCommand))]
        private FileSystemItem? selectedItem;

        [ObservableProperty]
        private string statusMessage;

        private string? _clipboardPath;
        private bool _isCutOperation;

        // Dialog interactions
        public Func<string, Task<(bool Confirmed, string Path, string Format)>>? ShowCompressDialog { get; set; }
        public Func<string, Task<(bool Confirmed, string Path)>>? ShowExtractDialog { get; set; }
        public Func<string, Task<bool>>? ShowConfirmDialog { get; set; }
        public Func<string, Task<(bool Confirmed, string NewName)>>? ShowRenameDialog { get; set; }

        public MainWindowViewModel()
        {
            Roots = new ObservableCollection<DirectoryNode>();
            CurrentItems = new ObservableCollection<FileSystemItem>();
            StatusMessage = "Ready";
            LoadDrives();
        }

        private void LoadDrives()
        {
            Roots.Clear();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    Roots.Add(new DirectoryNode(drive.RootDirectory.FullName));
                }
            }
        }

        partial void OnSelectedFolderChanged(DirectoryNode? value)
        {
            if (value != null)
            {
                LoadFiles(value.FullPath);
            }
        }

        private void LoadFiles(string path)
        {
            CurrentItems.Clear();
            StatusMessage = $"Loading {path}...";
            
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var dirInfo = new DirectoryInfo(path);

                // Add Directories
                foreach (var dir in dirInfo.GetDirectories())
                {
                    if ((dir.Attributes & FileAttributes.Hidden) == 0)
                    {
                        CurrentItems.Add(new FileSystemItem
                        {
                            Name = dir.Name,
                            FullPath = dir.FullName,
                            IsDirectory = true,
                            DateModified = dir.LastWriteTime,
                            Type = "File Folder",
                            Size = 0
                        });
                    }
                }

                // Add Files
                foreach (var file in dirInfo.GetFiles())
                {
                    if ((file.Attributes & FileAttributes.Hidden) == 0)
                    {
                        CurrentItems.Add(new FileSystemItem
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            IsDirectory = false,
                            DateModified = file.LastWriteTime,
                            Type = file.Extension.ToUpper() + " File",
                            Size = file.Length
                        });
                    }
                }
                
                if (CurrentItems.Count == 0)
                    StatusMessage = $"{path} is empty.";
                else
                    StatusMessage = $"{CurrentItems.Count} items in {path}";
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = $"Access Denied to {path}";
            }
            catch (DirectoryNotFoundException)
            {
                StatusMessage = $"Directory not found: {path}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading files: {ex.Message}";
            }
        }

        [RelayCommand]
        public void OpenItem(FileSystemItem item)
        {
            if (item == null) return;

            if (item.IsDirectory)
            {
                LoadFiles(item.FullPath);
            }
            else
            {
                try
                {
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(item.FullPath)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Cannot open file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public async Task CompressItem()
        {
            if (SelectedItem == null || ShowCompressDialog == null) return;
            
            string source = SelectedItem.FullPath;
            string defaultName = Path.GetFileNameWithoutExtension(source);
            string defaultPath = Path.Combine(Path.GetDirectoryName(source) ?? "", defaultName);

            var result = await ShowCompressDialog(defaultPath);
            if (!result.Confirmed) return;

            string dest = result.Path;
            string format = result.Format;
            
            StatusMessage = $"Compressing to {format}...";
            
            await Task.Run(() =>
            {
                try
                {
                    if (format == "Rar")
                    {
                         string rarPath = GetRarExecutablePath();
                         if (string.IsNullOrEmpty(rarPath))
                         {
                             throw new NotSupportedException("WinRAR (rar.exe) not found. Please install WinRAR to create .rar archives.");
                         }
                         
                         var args = $"a -r \"{dest}\" \"{source}\"";
                         
                         var psi = new ProcessStartInfo
                         {
                             FileName = rarPath,
                             Arguments = args,
                             UseShellExecute = false,
                             CreateNoWindow = true,
                             RedirectStandardOutput = true,
                             RedirectStandardError = true
                         };
                         
                         using (var process = Process.Start(psi))
                         {
                             if (process == null) throw new Exception("Could not start RAR process.");
                             process.WaitForExit();
                             if (process.ExitCode != 0)
                             {
                                 string err = process.StandardError.ReadToEnd();
                                 throw new Exception($"RAR exited with code {process.ExitCode}: {err}");
                             }
                         }
                    }
                    else if (format == "Zip")
                    {
                        using (var archive = ZipArchive.Create())
                        {
                            if (SelectedItem.IsDirectory) 
                            {
                                archive.AddAllFromDirectory(source);
                            }
                            else 
                            {
                                archive.AddEntry(SelectedItem.Name, source);
                            }
                            archive.SaveTo(dest, CompressionType.Deflate);
                        }
                    }
                    else if (format == "7Zip") 
                    {
                        string sevenZipPath = GetSevenZipExecutablePath();
                        if (string.IsNullOrEmpty(sevenZipPath))
                        {
                            throw new NotSupportedException("7-Zip (7z.exe) not found. Please install 7-Zip.");
                        }

                        // 7z a -t7z "archive.7z" "source"
                        var args = $"a -t7z \"{dest}\" \"{source}\" -mx=9";

                        var psi = new ProcessStartInfo
                        {
                            FileName = sevenZipPath,
                            Arguments = args,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        using (var process = Process.Start(psi))
                        {
                            if (process == null) throw new Exception("Could not start 7-Zip process.");
                            process.WaitForExit();
                            if (process.ExitCode != 0)
                            {
                                string err = process.StandardError.ReadToEnd();
                                throw new Exception($"7-Zip exited with code {process.ExitCode}: {err}");
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"Format {format} is not supported.");
                    }
                }
                catch 
                {
                    throw; // Preserves stack trace
                }
            }).ContinueWith(t => 
            {
                if (t.IsFaulted)
                    StatusMessage = $"Compression failed: {t.Exception?.InnerException?.Message ?? t.Exception?.Message}";
                else
                    StatusMessage = $"Compressed to {dest}";
                    
                Dispatcher.UIThread.Post(Refresh); 
            });
        }
        
        private string GetRarExecutablePath()
        {
            if (Path.DirectorySeparatorChar == '\\') 
            {
                string[] candidates = {
                    @"C:\Program Files\WinRAR\Rar.exe",
                    @"C:\Program Files (x86)\WinRAR\Rar.exe"
                };
                foreach (var p in candidates)
                {
                    if (File.Exists(p)) return p;
                }
            }
            return "rar"; 
        }
        
        private string GetSevenZipExecutablePath()
        {
            if (Path.DirectorySeparatorChar == '\\') 
            {
                string[] candidates = {
                    @"C:\Program Files\7-Zip\7z.exe",
                    @"C:\Program Files (x86)\7-Zip\7z.exe"
                };
                foreach (var p in candidates)
                {
                    if (File.Exists(p)) return p;
                }
            }
            return "7z"; 
        }

        [RelayCommand]
        public async Task ExtractItem()
        {
            if (SelectedItem == null || SelectedItem.IsDirectory || ShowExtractDialog == null) return;
            
            string source = SelectedItem.FullPath;
            string defaultDest = Path.Combine(Path.GetDirectoryName(source) ?? "", Path.GetFileNameWithoutExtension(source));
            
            var result = await ShowExtractDialog(defaultDest);
            if (!result.Confirmed) return;
            
            string destFolder = result.Path;

            StatusMessage = "Extracting...";
            
            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(destFolder);
                    using (var archive = ArchiveFactory.Open(source))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(destFolder, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                catch
                {
                     throw;
                }
            }).ContinueWith(t => 
            {
                if (t.IsFaulted)
                    StatusMessage = $"Extraction failed: {t.Exception?.InnerException?.Message ?? t.Exception?.Message}";
                else
                    StatusMessage = $"Extracted to {destFolder}";

                Dispatcher.UIThread.Post(Refresh);
            });
        }

        [RelayCommand]
        public async Task DeleteItem()
        {
            if (SelectedItem == null) return;
            if (ShowConfirmDialog == null) 
            {
                StatusMessage = "Error: Confirmation dialog not configured.";
                return;
            }
            
            var path = SelectedItem.FullPath;
            var isDir = SelectedItem.IsDirectory;
            
            var confirmed = await ShowConfirmDialog($"Are you sure you want to delete '{(isDir ? "Folder" : "File")}: {SelectedItem.Name}'?");
            if (!confirmed) return;
            
            try
            {
                if (isDir) Directory.Delete(path, true);
                else File.Delete(path);
                
                Refresh();
                StatusMessage = $"Deleted {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Delete failed: {ex.Message}";
            }
        }

        [RelayCommand]
        public void CreateFolder()
        {
            if (SelectedFolder == null) 
            {
                StatusMessage = "Please select a folder in the tree view first.";
                return;
            }
            try
            {
                string path = Path.Combine(SelectedFolder.FullPath, "New Folder");
                int i = 1;
                while (Directory.Exists(path))
                {
                    path = Path.Combine(SelectedFolder.FullPath, $"New Folder ({i++})");
                }
                Directory.CreateDirectory(path);
                Refresh();
                StatusMessage = "Folder created.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; }
        }

        [RelayCommand]
        public void CreateFile(string type)
        {
            if (SelectedFolder == null)
            {
                StatusMessage = "Please select a folder in the tree view first.";
                return;
            }
            string ext = type switch
            {
                "Text" => ".txt",
                "Word" => ".docx",
                "Excel" => ".xlsx",
                "PowerPoint" => ".pptx",
                _ => ".txt"
            };
            
            string path = Path.Combine(SelectedFolder.FullPath, $"New File{ext}");
            try
            {
                int i = 1;
                while (File.Exists(path))
                {
                     path = Path.Combine(SelectedFolder.FullPath, $"New File ({i++}){ext}");
                }

                File.WriteAllText(path, ""); 
                Refresh();
                StatusMessage = $"File created: {Path.GetFileName(path)}";
            }
            catch (Exception ex) { StatusMessage = ex.Message; }
        }
        
        [RelayCommand]
        public async Task RenameItem() 
        {
             if (SelectedItem == null) return;
             if (ShowRenameDialog == null)
             {
                 StatusMessage = "Error: Rename dialog not configured.";
                 return;
             }

             var oldPath = SelectedItem.FullPath;
             var oldName = SelectedItem.Name;

             var (confirmed, newName) = await ShowRenameDialog(oldName);
             if (!confirmed || string.IsNullOrWhiteSpace(newName) || newName == oldName) return;

             try 
             {
                 var dir = Path.GetDirectoryName(oldPath);
                 if (dir == null) return;
                 var newPath = Path.Combine(dir, newName);

                 if (SelectedItem.IsDirectory)
                     Directory.Move(oldPath, newPath);
                 else
                     File.Move(oldPath, newPath);

                 Refresh();
                 StatusMessage = $"Renamed to {newName}";
             }
             catch (Exception ex)
             {
                 StatusMessage = $"Rename failed: {ex.Message}";
             }
        }

        // --- Clipboard Operations ---

        [RelayCommand]
        public void CutItem()
        {
            if (SelectedItem == null) return;
            _clipboardPath = SelectedItem.FullPath;
            _isCutOperation = true;
            StatusMessage = $"Cut: {SelectedItem.Name}";
            PasteItemCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        public void CopyItem()
        {
            if (SelectedItem == null) return;
            _clipboardPath = SelectedItem.FullPath;
            _isCutOperation = false;
            StatusMessage = $"Copied: {SelectedItem.Name}";
            PasteItemCommand.NotifyCanExecuteChanged();
        }

        private bool CanPasteItem()
        {
            return !string.IsNullOrEmpty(_clipboardPath) && SelectedItem != null && SelectedItem.IsDirectory;
        }

        [RelayCommand(CanExecute = nameof(CanPasteItem))]
        public void PasteItem()
        {
            if (string.IsNullOrEmpty(_clipboardPath) || SelectedItem == null || !SelectedItem.IsDirectory) return;

            string destFolder = SelectedItem.FullPath;
            string fileName = Path.GetFileName(_clipboardPath);
            string destPath = Path.Combine(destFolder, fileName);

            try
            {
                if (_isCutOperation)
                {
                    // Move
                    if (File.Exists(_clipboardPath))
                    {
                        if (File.Exists(destPath)) throw new Exception("File already exists in destination.");
                        File.Move(_clipboardPath, destPath);
                    }
                    else if (Directory.Exists(_clipboardPath))
                    {
                        if (Directory.Exists(destPath)) throw new Exception("Folder already exists in destination.");
                        Directory.Move(_clipboardPath, destPath);
                    }
                    else
                    {
                         throw new FileNotFoundException("Source file/folder not found.");
                    }
                    
                    _clipboardPath = null;
                    _isCutOperation = false;
                    PasteItemCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    // Copy
                    if (File.Exists(_clipboardPath))
                    {
                        File.Copy(_clipboardPath, destPath, true);
                    }
                    else if (Directory.Exists(_clipboardPath))
                    {
                        CopyDirectory(_clipboardPath, destPath);
                    }
                }

                StatusMessage = $"Pasted to {destFolder}";
                Refresh(); 
                if (SelectedFolder != null && SelectedFolder.FullPath == destFolder)
                {
                     LoadFiles(destFolder);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Paste Error: {ex.Message}";
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private void Refresh()
        {
             if (SelectedFolder != null) LoadFiles(SelectedFolder.FullPath);
        }
    }
}