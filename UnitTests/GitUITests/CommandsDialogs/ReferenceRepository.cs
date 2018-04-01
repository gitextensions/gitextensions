using System;
using CommonTestUtils;
using GitCommands;

namespace GitUITests.CommandsDialogs
{
    public class ReferenceRepository : IDisposable
    {
        private static GitModuleTestHelper _moduleTestHelper;
        private static string _commitHash;

        public ReferenceRepository()
        {
            _moduleTestHelper = new GitModuleTestHelper();

            using (var repository = new LibGit2Sharp.Repository(_moduleTestHelper.Module.WorkingDir))
            {
                _moduleTestHelper.CreateRepoFile("A.txt", "A");
                repository.Index.Add("A.txt");

                var message = "A commit message";
                var author = new LibGit2Sharp.Signature("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
                var committer = author;
                var options = new LibGit2Sharp.CommitOptions();
                var commit = repository.Commit(message, author, committer, options);
                _commitHash = commit.Id.Sha;
            }
        }

        public GitModule Module => _moduleTestHelper.Module;

        public string CommitHash => _commitHash;

        public void Reset()
        {
            // Undo potential impact from earlier tests
            using (var repository = new LibGit2Sharp.Repository(Module.WorkingDir))
            {
                var options = new LibGit2Sharp.CheckoutOptions();
                repository.Reset(LibGit2Sharp.ResetMode.Hard, (LibGit2Sharp.Commit)repository.Lookup(CommitHash, LibGit2Sharp.ObjectType.Commit), options);
                repository.RemoveUntrackedFiles();
            }

            CommitHelper.SetCommitMessage(Module, commitMessageText: null, amendCommit: false);
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
