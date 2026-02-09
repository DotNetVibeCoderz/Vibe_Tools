using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TerminalNet.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ObservableCollection<TerminalSessionViewModel> Sessions { get; } = new();
        
        private TerminalSessionViewModel? _selectedSession;
        public TerminalSessionViewModel? SelectedSession
        {
            get => _selectedSession;
            set => this.RaiseAndSetIfChanged(ref _selectedSession, value);
        }

        public ICommand AddTabCommand { get; }
        public ICommand CloseTabCommand { get; }

        public MainViewModel()
        {
            AddTabCommand = ReactiveCommand.Create(AddTab);
            CloseTabCommand = ReactiveCommand.Create<TerminalSessionViewModel>(CloseTab);

            // Start with one session
            AddTab();
        }

        private void AddTab()
        {
            var session = new TerminalSessionViewModel();
            session.Header = $"Session {Sessions.Count + 1}";
            Sessions.Add(session);
            SelectedSession = session;
        }

        private void CloseTab(TerminalSessionViewModel session)
        {
            if (session != null && Sessions.Contains(session))
            {
                Sessions.Remove(session);
                if (Sessions.Count == 0)
                {
                    // If all tabs closed, maybe close app or create new tab?
                    // Let's create new for now.
                    AddTab();
                }
            }
        }
    }
}