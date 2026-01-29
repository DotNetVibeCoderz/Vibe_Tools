using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JunkCleaner
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly CleanerEngine _engine;
        private string _statusMessage = "Ready to clean.";
        private string _junkSizeText = "0 MB";
        private bool _isBusy;
        private int _progressValue;

        public MainViewModel()
        {
            _engine = new CleanerEngine();
            _engine.OnProgress += (msg) => StatusMessage = msg;
            ScanCommand = new RelayCommand(async _ => await Scan());
            CleanCommand = new RelayCommand(async _ => await Clean());
            Logs = new ObservableCollection<string>();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged();
                
                    Application.Current.Dispatcher.Invoke(() => {
                        Logs.Add(value); 
                        if (Logs.Count > 100)
                            Logs.RemoveAt(0);
                    });
                }
        }

        public string JunkSizeText
        {
            get => _junkSizeText;
            set { _junkSizeText = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }
        
        public int ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Logs { get; }

        public ICommand ScanCommand { get; }
        public ICommand CleanCommand { get; }

        private async Task Scan()
        {
            IsBusy = true;
            ProgressValue = 0;
            Logs.Clear();
            StatusMessage = "Scanning System...";
            
            long tempSize = await _engine.ScanTempFiles();
            ProgressValue = 50;
            long browserSize = await _engine.ScanBrowserCache();
            ProgressValue = 100;

            long total = tempSize + browserSize;
            JunkSizeText = $"{(total / 1024.0 / 1024.0):F2} MB";
            
            StatusMessage = "Scan Complete. Ready to clean.";
            IsBusy = false;
        }

        private async Task Clean()
        {
            IsBusy = true;
            ProgressValue = 0;
            StatusMessage = "Cleaning started...";

            await _engine.CleanTempFiles();
            ProgressValue = 50;
            await _engine.CleanBrowserCache();
            ProgressValue = 80;
            await _engine.CleanRecycleBin();
            ProgressValue = 100;

            StatusMessage = "Cleanup Finished!";
            JunkSizeText = "0 MB";
            IsBusy = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}