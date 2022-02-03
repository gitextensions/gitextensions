using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GitCommands;
using GitCommands.Config;
using LibGit2Sharp;
using Remote = LibGit2Sharp.Remote;

namespace CommonTestUtils
{
    public class ReferenceRepository : IDisposable
    {
        private GitModuleTestHelper _moduleTestHelper;
        private string _commitHash;

        public ReferenceRepository()
        {
            _moduleTestHelper = new GitModuleTestHelper();

            CreateCommit("A commit message", "A");
        }

        /// <summary>
        /// Reset the repo if possible, if it is null or reset throws create a new.
        /// </summary>
        /// <param name="refRepo">The repo to reset, possibly null.</param>
        public static void ResetRepo(ref ReferenceRepository refRepo)
        {
            if (refRepo is null)
            {
                refRepo = new ReferenceRepository();
            }
            else
            {
                try
                {
                    refRepo.Reset();
                }
                catch (LibGit2Sharp.LockedFileException)
                {
                    // the index is locked; this might be due to a concurrent or crashed process
                    refRepo = new ReferenceRepository();
                    Trace.WriteLine("Repo is locked, creating new");
                }
            }
        }

        public GitModule Module => _moduleTestHelper.Module;

        public string CommitHash => _commitHash;

        private const string _fileName = "A.txt";

        private string Commit(Repository repository, string commitMessage)
        {
            LibGit2Sharp.Signature author = new("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
            var committer = author;
            LibGit2Sharp.CommitOptions options = new() { PrettifyMessage = false };
            var commit = repository.Commit(commitMessage, author, committer, options);
            return commit.Id.Sha;
        }

        public void CreateBranch(string branchName, string commitHash, bool allowOverwrite = false)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            repository.Branches.Add(branchName, commitHash, allowOverwrite);
            Console.WriteLine($"Created branch: {commitHash}, message: {branchName}");
        }

        public string CreateCommit(string commitMessage, string content = null)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(_fileName, content ?? commitMessage);
            repository.Index.Add(_fileName);

            _commitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {_commitHash}, message: {commitMessage}");
            return _commitHash;
        }

        public string CreateCommit(string commitMessage, string content1, string fileName1, string content2, string fileName2)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(fileName1, content1);
            repository.Index.Add(fileName1);
            _moduleTestHelper.CreateRepoFile(fileName2, content2);
            repository.Index.Add(fileName2);

            _commitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {_commitHash}, message: {commitMessage}");
            return _commitHash;
        }

        public string CreateRepoFile(string fileName, string fileContent) => _moduleTestHelper.CreateRepoFile(fileName, fileContent);

        public void CreateAnnotatedTag(string tagName, string commitHash, string message)
        {
            LibGit2Sharp.Signature author = new("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);

            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            repository.Tags.Add(tagName, commitHash, author, message);
        }

        public void CreateTag(string tagName, string commitHash, bool allowOverwrite = false)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            repository.Tags.Add(tagName, commitHash, allowOverwrite);
        }

        public void CheckoutRevision()
        {
            using LibGit2Sharp.Repository repository = new(Module.WorkingDir);
            Commands.Checkout(repository, CommitHash, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
        }

        public void CheckoutBranch(string branchName)
        {
            using LibGit2Sharp.Repository repository = new(Module.WorkingDir);
            Commands.Checkout(repository, branchName, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
        }

        public void CreateRemoteForMasterBranch()
        {
            using LibGit2Sharp.Repository repository = new(Module.WorkingDir);
            repository.Network.Remotes.Add("origin", "http://useless.url");
            Remote remote = repository.Network.Remotes["origin"];

            var masterBranch = repository.Branches["master"];

            repository.Branches.Update(masterBranch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = masterBranch.CanonicalName);
        }

        public void Fetch(string remoteName)
        {
            using LibGit2Sharp.Repository repository = new(Module.WorkingDir);
            LibGit2Sharp.FetchOptions options = new();
            Commands.Fetch(repository, remoteName, Array.Empty<string>(), options, null);
        }

        private void Reset()
        {
            // Undo potential impact from earlier tests
            using (LibGit2Sharp.Repository repository = new(Module.WorkingDir))
            {
                LibGit2Sharp.CheckoutOptions options = new();
                repository.Reset(LibGit2Sharp.ResetMode.Hard, (LibGit2Sharp.Commit)repository.Lookup(CommitHash, LibGit2Sharp.ObjectType.Commit), options);
                repository.RemoveUntrackedFiles();

                var remoteNames = repository.Network.Remotes.Select(remote => remote.Name).ToArray();
                foreach (var remoteName in remoteNames)
                {
                    repository.Network.Remotes.Remove(remoteName);
                }

                repository.Config.Set(SettingKeyString.UserName, "author");
                repository.Config.Set(SettingKeyString.UserEmail, "author@mail.com");
            }

            new CommitMessageManager(Module.WorkingDirGitDir, Module.CommitEncoding).ResetCommitMessage();
        }

        public void Stash(string stashMessage, string content = null)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(_fileName, content ?? stashMessage);
            repository.Index.Add(_fileName);

            LibGit2Sharp.Signature author = new("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
            Stash stash = repository.Stashes.Add(author, stashMessage);
            Console.WriteLine($"Created stash: {stash.Index.Sha}, message: {stashMessage}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _moduleTestHelper.Dispose();
        }
    }
}
