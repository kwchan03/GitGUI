using GitGUI.Core;
using GitGUI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GitGUI.ViewModels
{
    public class OperationViewModel : INotifyPropertyChanged
    {
        private readonly IGitService _git;
        public ObservableCollection<CommitInfo> Commits { get; } = new ObservableCollection<CommitInfo>();
        public ObservableCollection<BranchInfo> Branches { get; } = new ObservableCollection<BranchInfo>();
        public ObservableCollection<ChangeItem> StagedChanges { get; } = new ObservableCollection<ChangeItem>();
        public ObservableCollection<ChangeItem> UnstagedChanges { get; } = new ObservableCollection<ChangeItem>();

        private string _repoPath = string.Empty;
        private string _outputLog = string.Empty;
        private string _commitMessage = string.Empty;
        private const int MaxLogLength = 10000; // Maximum number of characters in the log
        private const int MaxLogLines = 1000; // Maximum number of lines in the log

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The list of recent commits in the opened repo.
        /// </summary>
        public ICommand BrowseFolderCommand { get; }
        public ICommand OpenRepoCommand { get; }
        public ICommand CreateRepoCommand { get; }
        public ICommand LoadCommitsCommand { get; }
        public ICommand ClearLogCommand { get; }

        public ICommand LoadBranchesCommand { get; }
        public ICommand CheckoutBranchCommand { get; }
        public ICommand CreateBranchCommand { get; }
        public ICommand MergeBranchCommand { get; }

        public ICommand RefreshChangesCommand { get; }
        public ICommand StageCommand { get; }
        public ICommand UnstageCommand { get; }
        public ICommand CommitCommand { get; }

        /// Command to clear the output log
        /// </summary>

        public OperationViewModel(IGitService git)
        {
            _git = git ?? throw new ArgumentNullException(nameof(git));

            BrowseFolderCommand = new RelayCommand(_ => ExecuteBrowseFolder());
            OpenRepoCommand = new RelayCommand(_ => ExecuteOpenRepo());
            ClearLogCommand = new RelayCommand(_ => ClearLog());
            CreateRepoCommand = new RelayCommand(_ => ExecuteCreateRepo(), _ => !string.IsNullOrWhiteSpace(RepoPath));

            LoadBranchesCommand = new RelayCommand(_ => ExecuteLoadBranches());
            CheckoutBranchCommand = new RelayCommand(_ => ExecuteCheckoutBranch(), _ => SelectedBranch != null);
            CreateBranchCommand = new RelayCommand(_ => ExecuteCreateBranch(), _ => !string.IsNullOrWhiteSpace(NewBranchName));
            MergeBranchCommand = new RelayCommand(_ => ExecuteMergeBranch(), _ => SelectedBranch != null);

            RefreshChangesCommand = new RelayCommand(_ => ExecuteRefreshChanges());
            StageCommand = new RelayCommand(_ => ExecuteStageFile());
            UnstageCommand = new RelayCommand(_ => ExecuteUnstageFile());
            CommitCommand = new RelayCommand(_ => ExecuteCommit(), _ => !string.IsNullOrWhiteSpace(CommitMessage));
        }

        /// <summary>
        /// The folder path of the repository to open or init.
        /// </summary>
        public string RepoPath
        {
            get => _repoPath;
            set
            {
                if (_repoPath != value)
                {
                    _repoPath = value;
                    OnPropertyChanged();
                    // Re-evaluate CanExecute for commands that depend on RepoPath
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private BranchInfo? _selectedBranch;
        public BranchInfo? SelectedBranch
        {
            get => _selectedBranch;
            set { _selectedBranch = value; OnPropertyChanged(); }
        }

        private string _newBranchName = "";
        public string NewBranchName
        {
            get => _newBranchName;
            set { _newBranchName = value; OnPropertyChanged(); }
        }

        private string _currentBranch = string.Empty;
        public string CurrentBranch
        {
            get => _currentBranch;
            private set
            {
                if (_currentBranch != value)
                {
                    _currentBranch = value;
                    OnPropertyChanged();
                }
            }
        }

        private ChangeItem _selectedChange;
        public ChangeItem SelectedChange
        {
            get => _selectedChange;
            set
            {
                if (_selectedChange != value)
                {
                    _selectedChange = value;
                    OnPropertyChanged();
                }
            }
        }
        public void SetSelectedChange(ChangeItem changeItem)
        {
            SelectedChange = changeItem;
        }

        public string CommitMessage
        {
            get => _commitMessage;
            set
            {
                if (_commitMessage != value)
                {
                    _commitMessage = value;
                    OnPropertyChanged();
                    // Re-evaluate whether CommitCommand can execute
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string OutputLog
        {
            get => _outputLog;
            private set
            {
                _outputLog = value;
                OnPropertyChanged();
            }
        }

        public void ClearLog()
        {
            OutputLog = string.Empty;
            AppendLog("Log cleared");
        }

        /// <summary>
        /// Gets the last N lines from the log
        /// </summary>
        /// <param name="count">Number of lines to retrieve</param>
        /// <returns>The last N lines of the log</returns>
        public string GetLastLines(int count)
        {
            if (string.IsNullOrEmpty(OutputLog))
                return string.Empty;

            var lines = OutputLog.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, lines.Skip(Math.Max(0, lines.Length - count)));
        }

        private void ExecuteBrowseFolder()
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Select or create a folder",
                ShowNewFolderButton = true
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                RepoPath = dlg.SelectedPath;
        }

        private void ExecuteOpenRepo()
        {
            try
            {
                bool existed = _git.OpenRepository(RepoPath);
                AppendLog(existed
                    ? $"Opened existing repo at '{RepoPath}'."
                    : $"No repo here, you may want to Create.");
                ExecuteLoadCommits();
                ExecuteLoadBranches();
                ExecuteRefreshChanges();
            }
            catch (Exception ex)
            {
                AppendLog($"Error opening repo: {ex.Message}");
            }
        }

        private void ExecuteCreateRepo()
        {
            try
            {
                _git.CreateRepository(RepoPath);
                AppendLog($"Created (or opened) repo at '{RepoPath}'.");
                ExecuteLoadCommits();
                ExecuteLoadBranches();
                ExecuteRefreshChanges();
            }
            catch (Exception ex)
            {
                AppendLog($"Error creating repo: {ex.Message}");
            }
        }

        private void ExecuteLoadCommits()
        {
            Commits.Clear();
            try
            {
                foreach (var commit in _git.GetCommitLog())
                    Commits.Add(commit);

                AppendLog($"Loaded {Commits.Count} commits.");
            }
            catch (Exception ex)
            {
                AppendLog($"Error loading commits: {ex.Message}");
            }
        }

        private void ExecuteLoadBranches()
        {
            Branches.Clear();
            foreach (var b in _git.GetBranches())
                Branches.Add(b);
            var current = Branches.FirstOrDefault(b => b.IsCurrent);
            CurrentBranch = current?.Name ?? "<none>";
            AppendLog($"Found {Branches.Count} branches. Current: {CurrentBranch}");
        }

        private void ExecuteCheckoutBranch()
        {
            if (SelectedBranch == null) return;
            try
            {
                _git.CheckoutBranch(SelectedBranch.Name);
                AppendLog($"Checked out branch '{SelectedBranch.Name}'.");
                ExecuteLoadBranches();
                ExecuteLoadCommits();
                ExecuteRefreshChanges();
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., uncommitted changes)
                AppendLog($"Error checking out branch: {ex.Message}");
            }
        }

        private void ExecuteCreateBranch()
        {
            _git.CreateBranch(NewBranchName);
            AppendLog($"Created and checked out new branch '{NewBranchName}'.");
            NewBranchName = "";
            ExecuteLoadBranches();
            ExecuteRefreshChanges();
        }

        private void ExecuteMergeBranch()
        {
            if (SelectedBranch == null) return;
            _git.MergeBranch(SelectedBranch.Name);
            AppendLog($"Merged branch '{SelectedBranch.Name}' into current.");
            ExecuteLoadBranches();
            ExecuteLoadCommits();
        }

        private void ExecuteStageFile()
        {
            if (SelectedChange != null)
            {
                try
                {
                    _git.StageFile(SelectedChange.FilePath);  // Stage the file
                    AppendLog($"Staged file: {SelectedChange.FilePath}");

                    // Refresh the changes to reflect the updated state
                    ExecuteRefreshChanges();  // Refresh the staged and unstaged lists
                }
                catch (Exception ex)
                {
                    AppendLog($"Error staging file: {ex.Message}");
                }
            }
        }

        // Unstage a file
        private void ExecuteUnstageFile()
        {
            if (SelectedChange != null)
            {
                try
                {
                    _git.UnstageFile(SelectedChange.FilePath);  // Unstage the file
                    AppendLog($"Unstaged file: {SelectedChange.FilePath}");

                    // Refresh the changes to reflect the updated state
                    ExecuteRefreshChanges();  // Refresh the staged and unstaged lists
                }
                catch (Exception ex)
                {
                    AppendLog($"Error unstaging file: {ex.Message}");
                }
            }
        }

        private void ExecuteCommit()
        {
            try
            {
                _git.Commit(CommitMessage);
                AppendLog($"Committed: {CommitMessage}");

                // Clear the message box
                CommitMessage = string.Empty;

                // Refresh both commits list and changes list
                ExecuteLoadCommits();
                ExecuteRefreshChanges();
            }
            catch (Exception ex)
            {
                AppendLog($"Error committing: {ex.Message}");
            }
        }

        private void ExecuteRefreshChanges()
        {
            StagedChanges.Clear();
            UnstagedChanges.Clear();

            try
            {
                var (stagedChanges, unstagedChanges) = _git.GetChanges();
                foreach (var change in stagedChanges)
                    StagedChanges.Add(change);
                foreach (var change in unstagedChanges)
                    UnstagedChanges.Add(change);

                AppendLog($"Found {StagedChanges.Count} staged changes and {UnstagedChanges.Count} unstaged changes.");
            }
            catch (Exception ex)
            {
                AppendLog($"Error fetching changes: {ex.Message}");
            }
        }

        private void AppendLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"{timestamp} – {message}{Environment.NewLine}";

            // If adding this entry would exceed the maximum length, trim the log
            if (OutputLog.Length + logEntry.Length > MaxLogLength)
            {
                var lines = OutputLog.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var trimmedLines = lines.Skip(lines.Length - MaxLogLines).ToArray();
                OutputLog = string.Join(Environment.NewLine, trimmedLines) + Environment.NewLine;
            }

            OutputLog += logEntry;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
