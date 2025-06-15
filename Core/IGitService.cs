﻿using GitGUI.Models;

namespace GitGUI.Core
{
    public interface IGitService
    {
        void CreateRepository(string path);
        IEnumerable<CommitInfo> GetCommitLog(int maxCount = 50);
        bool OpenRepository(string repositoryPath);

        IEnumerable<BranchInfo> GetBranches();
        void CheckoutBranch(string branchName);
        void CreateBranch(string newBranchName);
        void MergeBranch(string branchToMerge);
        (IEnumerable<ChangeItem> StagedChanges, IEnumerable<ChangeItem> UnstagedChanges) GetChanges();
        void StageFile(string relativePath);
        void UnstageFile(string relativePath);
        void Commit(string commitMessage);
    }
}
