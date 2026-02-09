using Avalonia.Media;
using System;

namespace TerminalNet.Models
{
    // Class untuk item di terminal (baris output atau input)
    public class TerminalItem
    {
        public string Text { get; set; } = "";
        public bool IsInput { get; set; } = false;
        public bool IsError { get; set; } = false;
        public bool IsWarning { get; set; } = false;
        
        // Properti warna dinamis berdasarkan tipe item
        public IBrush Foreground
        {
            get
            {
                if (IsInput) return Brushes.LimeGreen;
                if (IsError) return Brushes.Red;
                if (IsWarning) return Brushes.Yellow;
                return Brushes.LightGray;
            }
        }

        public TerminalItem(string text, bool isInput = false, bool isError = false, bool isWarning = false)
        {
            Text = text;
            IsInput = isInput;
            IsError = isError;
            IsWarning = isWarning;
        }
    }
}