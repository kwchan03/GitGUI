using GitGUI.Core;
using GitGUI.Models;
using LibGit2Sharp;
using System.Collections.ObjectModel;
using System.IO;

namespace GitGUI.Services
{
    public class GitLibService : IGitService
    {
        private Repository _repo;
        public ObservableCollection<CommitInfo> Commits { get; } = new ObservableCollection<CommitInfo>();
        public ObservableCollection<BranchInfo> Branches { get; } = new ObservableCollection<BranchInfo>();

        public bool OpenRepository(string path)
        {
            string gitDir = Path.Combine(path, ".git");
            if (!Repository.IsValid(path))
                throw new InvalidOperationException($"No git repository found at {path}");
            _repo = new Repository(path);
            return true;
        }

        public void CreateRepository(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // If there’s already a .git folder, just open it
            if (Repository.IsValid(path))
            {
                _repo = new Repository(path);
            }
            else
            {
                // Initialize a brand-new repo
                Repository.Init(path);
                _repo = new Repository(path);
            }
            CreateInitialCommit();
        }

        private void CreateInitialCommit()
        {
            try
            {
                // Create a README.md file or another placeholder file to add to the initial commit
                var filePath = Path.Combine(_repo.Info.WorkingDirectory, "README.md");

                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "# Initial Commit\nThis is the first commit."); // Add content to README
                }

                // Stage the newly created file
                Commands.Stage(_repo, filePath);

                // Create the initial commit with a commit message
                var signature = _repo.Config.BuildSignature(DateTimeOffset.Now);
                _repo.Commit("Initial commit", signature, signature);

            }
            catch (Exception ex)
            {
                // Throw exception to be caught by the ViewModel
                throw new InvalidOperationException($"Error creating initial commit: {ex.Message}", ex);
            }
        }

        public IEnumerable<BranchInfo> GetBranches()
        {
            if (_repo == null) throw new InvalidOperationException("Repository not opened or created");
            return _repo.Branches
                        .Select(b => new BranchInfo
                        {
                            Name = b.FriendlyName,
                            IsCurrent = b.IsCurrentRepositoryHead,
                            TipSha = b.Tip.Sha.Substring(0, 7)
                        });
        }

        public void CheckoutBranch(string branchName)
        {
            if (_repo == null)
                throw new InvalidOperationException("Repository not opened or created");

            // 1) Check for uncommitted changes
            var status = _repo.RetrieveStatus(new StatusOptions());
            if (status.IsDirty)
                throw new InvalidOperationException(
                    "You have uncommitted changes. Please commit or stash them before switching branches.");

            // 2) Perform the checkout
            Commands.Checkout(_repo, branchName);
        }

        public void CreateBranch(string newBranchName)
        {
            if (_repo == null) throw new InvalidOperationException("Repository not opened or created");
            var branch = _repo.CreateBranch(newBranchName);
            Commands.Checkout(_repo, branch);
        }

        public void MergeBranch(string branchToMerge)
        {
            if (_repo == null) throw new InvalidOperationException("Repository not opened or created");
            var target = _repo.Branches[branchToMerge];
            if (target == null) throw new InvalidOperationException($"Branch '{branchToMerge}' not found");
            var merger = _repo.Config.BuildSignature(DateTimeOffset.Now);
            _repo.Merge(target, merger);
        }

        public (IEnumerable<ChangeItem> StagedChanges, IEnumerable<ChangeItem> UnstagedChanges) GetChanges()
        {
            if (_repo == null)
                throw new InvalidOperationException("Repository not opened");

            var status = _repo.RetrieveStatus();  // Get the status of all files

            // Separate staged and unstaged changes
            var stagedChanges = status.Where(e => IsFileStaged(e.State))
                                      .Select(e => new ChangeItem
                                      {
                                          FilePath = e.FilePath,
                                          Status = MapStatus(e.State),
                                          IsStaged = true
                                      });

            var unstagedChanges = status.Where(e => !IsFileStaged(e.State))
                                        .Select(e => new ChangeItem
                                        {
                                            FilePath = e.FilePath,
                                            Status = MapStatus(e.State),
                                            IsStaged = false
                                        });

            return (stagedChanges, unstagedChanges);
        }

        private bool IsFileStaged(FileStatus status)
        {
            // Check if the file is staged in the index (added, modified, deleted, renamed)
            return status.HasFlag(FileStatus.NewInIndex) ||
                   status.HasFlag(FileStatus.ModifiedInIndex) ||
                   status.HasFlag(FileStatus.DeletedFromIndex) ||
                   status.HasFlag(FileStatus.RenamedInIndex);
        }

        private ChangeStatus MapStatus(FileStatus status)
        {
            if (status.HasFlag(FileStatus.NewInIndex)) return ChangeStatus.Added;  // File added to the index
            if (status.HasFlag(FileStatus.ModifiedInIndex)) return ChangeStatus.Modified;  // File modified in index
            if (status.HasFlag(FileStatus.DeletedFromIndex)) return ChangeStatus.Deleted;  // File deleted from the index
            if (status.HasFlag(FileStatus.RenamedInIndex)) return ChangeStatus.Renamed;  // File renamed in the index
            if (status.HasFlag(FileStatus.NewInWorkdir)) return ChangeStatus.Added;  // New file in workdir (not yet in index)
            if (status.HasFlag(FileStatus.ModifiedInWorkdir)) return ChangeStatus.Modified;  // File modified in workdir
            if (status.HasFlag(FileStatus.DeletedFromWorkdir)) return ChangeStatus.Deleted;  // File deleted from workdir
            if (status.HasFlag(FileStatus.RenamedInWorkdir)) return ChangeStatus.Renamed;  // File renamed in workdir
            if (status.HasFlag(FileStatus.Conflicted)) return ChangeStatus.Conflicted;  // File has merge conflict
            if (status.HasFlag(FileStatus.Ignored)) return ChangeStatus.Ignored;  // File is ignored

            return ChangeStatus.Untracked;  // Default, if the file isn't tracked yet
        }

        public void StageFile(string relativePath)
        {
            if (_repo == null) throw new InvalidOperationException("Repository not opened or created");
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(relativePath));
            Commands.Stage(_repo, relativePath);
        }

        public void UnstageFile(string relativePath)
        {
            if (_repo == null) throw new InvalidOperationException("Repository not opened or created");
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(relativePath));

            // Unstage the file
            Commands.Unstage(_repo, relativePath);
        }

        public void Commit(string message)
        {
            if (_repo == null) throw new InvalidOperationException();
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Commit message cannot be empty", nameof(message));

            // Read user.name and user.email from config, or fall back to placeholders
            var nameEntry = _repo.Config.Get<string>("user.name");
            var emailEntry = _repo.Config.Get<string>("user.email");
            var authorName = nameEntry?.Value ?? "Unknown";
            var authorEmail = emailEntry?.Value ?? "unknown@example.com";
            var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
            _repo.Commit(message, author, author);
        }

        public IEnumerable<CommitInfo> GetCommitLog(int maxCount = 50)
        {
            if (_repo == null)
                throw new InvalidOperationException("Repository not opened or created");
            return _repo.Commits
                        .Take(maxCount)
                        .Select(c => new CommitInfo
                        {
                            Sha = c.Sha,
                            Message = c.MessageShort,
                            AuthorName = c.Author.Name,
                            Date = c.Author.When.LocalDateTime
                        });
        }

    }
}