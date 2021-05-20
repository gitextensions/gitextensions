using System;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using LibGit2Sharp;
using Remote = LibGit2Sharp.Remote;

namespace CommonTestUtils
{
    public class ReferenceRepository : IDisposable
    {
        private static GitModuleTestHelper _moduleTestHelper;
        private static string _commitHash;

        public ReferenceRepository()
        {
            _moduleTestHelper = new GitModuleTestHelper();

            CreateCommit("A commit message", "A");
        }

        public GitModule Module => _moduleTestHelper.Module;

        public string CommitHash => _commitHash;

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
        }

        public void CreateCommit(string commitMessage, string content = null)
        {
            using LibGit2Sharp.Repository repository = new(_moduleTestHelper.Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile("A.txt", content ?? commitMessage);
            repository.Index.Add("A.txt");

            _commitHash = Commit(repository, commitMessage);
        }

        public string CreateRepoFile(string fileName, string fileContent) => _moduleTestHelper.CreateRepoFile(fileName, fileContent);

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

        public void CheckoutMaster()
        {
            using LibGit2Sharp.Repository repository = new(Module.WorkingDir);
            Commands.Checkout(repository, "master", new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
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

        public void Reset()
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
