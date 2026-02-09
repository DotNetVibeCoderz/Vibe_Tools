using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TerminalNet.Models;
using TerminalNet.Services;

namespace TerminalNet.ViewModels
{
    public class TerminalSessionViewModel : ReactiveObject
    {
        private string _header = "Terminal";
        private string _currentInput = "";
        private string _prompt;
        private TerminalEngine _engine;
        
        public string Header
        {
            get => _header;
            set => this.RaiseAndSetIfChanged(ref _header, value);
        }

        public string CurrentDirectory => _engine.CurrentDirectory;

        public ObservableCollection<TerminalItem> OutputItems { get; } = new();

        public string CurrentInput
        {
            get => _currentInput;
            set => this.RaiseAndSetIfChanged(ref _currentInput, value);
        }

        public string Prompt
        {
            get => _prompt;
            set => this.RaiseAndSetIfChanged(ref _prompt, value);
        }

        public ICommand ExecuteCommand { get; }

        public TerminalSessionViewModel()
        {
            _engine = new TerminalEngine();
            _prompt = $"{_engine.CurrentDirectory}>";
            
            // Command to execute input
            ExecuteCommand = ReactiveCommand.CreateFromTask(ProcessCommandAsync);

            // Welcome message
            OutputItems.Add(new TerminalItem("TerminalNet [Version 1.0.0]", isWarning: true));
            OutputItems.Add(new TerminalItem("(c) 2024 Gravicode Studios. All rights reserved.", isWarning: true));
            OutputItems.Add(new TerminalItem("Type 'help' to see available commands."));
            OutputItems.Add(new TerminalItem("")); // Empty line
        }

        private async Task ProcessCommandAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentInput)) return;

            string cmd = CurrentInput;
            
            // Add input line to display (Prompt + Command)
            OutputItems.Add(new TerminalItem($"{Prompt} {cmd}", isInput: true));
            
            _engine.AddToHistory(cmd);
            CurrentInput = ""; // Clear input immediately

            if (cmd.Trim().ToLower() == "clear")
            {
                OutputItems.Clear();
                // Tetap tampilkan prompt jika kosong
                return;
            }

            // Execute command via Engine
            string result = await _engine.ExecuteCommandAsync(cmd);
            
            if (!string.IsNullOrEmpty(result))
            {
                // Check if result is error (simple check)
                bool isError = result.StartsWith("Error:") || result.Contains("not found");
                OutputItems.Add(new TerminalItem(result, isError: isError));
            }

            // Update prompt because directory might have changed
            Prompt = $"{_engine.CurrentDirectory}>";
            this.RaisePropertyChanged(nameof(CurrentDirectory));
            
            // Auto scroll should happen here but requires view access or behavior
        }
    }
}