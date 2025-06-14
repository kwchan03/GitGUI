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

        public void StageFile(string relativePath)
        {
            if (_repo == null) throw new InvalidOperationException();
            Commands.Stage(_repo, relativePath);
        }

        public void Commit(string message, string authorName, string authorEmail)
        {
            if (_repo == null) throw new InvalidOperationException();

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