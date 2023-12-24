using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitCommands.Config;
using LibGit2Sharp;

namespace CommonTestUtils
{
    public class ReferenceRepository : IDisposable
    {
        public const string AuthorName = "GitUITests";
        public const string AuthorEmail = "unittests@gitextensions.com";
        public const string AuthorFullIdentity = $"{AuthorName} <{AuthorEmail}>";
        private readonly GitModuleTestHelper _moduleTestHelper = new();

        // We don't expect any failures so that we won't be switching to the main thread or showing messages
        public static Control DummyOwner { get; } = new();

        public ReferenceRepository(bool createCommit = true)
        {
            if (createCommit)
            {
                CreateCommit("A commit message", "A");
            }
        }

        /// <summary>
        /// Reset the repo if possible, if it is null or reset throws create a new.
        /// </summary>
        /// <param name="refRepo">The repo to reset, possibly null.</param>
        public static void ResetRepo([NotNull] ref ReferenceRepository? refRepo)
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
                catch (LockedFileException)
                {
                    // the index is locked; this might be due to a concurrent or crashed process
                    refRepo.Dispose();
                    refRepo = new ReferenceRepository();
                    Trace.WriteLine("Repo is locked, creating new");
                }
            }
        }

        public GitModule Module => _moduleTestHelper.Module;

        public string? CommitHash { get; private set; }

        private const string _fileName = "A.txt";

        private static void IndexAdd(Repository repository, string fileName)
        {
            repository.Index.Add(fileName);
            repository.Index.Write();
        }

        private static string Commit(Repository repository, string commitMessage)
        {
            Signature author = GetAuthorSignature();
            CommitOptions options = new() { PrettifyMessage = false };
            Commit commit = repository.Commit(commitMessage, author, author, options);
            repository.Index.Write();
            return commit.Id.Sha;
        }

        public void CreateBranch(string branchName, string commitHash, bool allowOverwrite = false)
        {
            using Repository repository = new(Module.WorkingDir);
            repository.Branches.Add(branchName, commitHash, allowOverwrite);
            Console.WriteLine($"Created branch: {commitHash}, message: {branchName}");
        }

        public string CreateCommit(string commitMessage, string content = null)
        {
            using Repository repository = new(Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(_fileName, content ?? commitMessage);
            IndexAdd(repository, _fileName);

            CommitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {CommitHash}, message: {commitMessage}");
            return CommitHash;
        }

        public string CreateCommit(string commitMessage, string content1, string fileName1, string? content2 = null, string? fileName2 = null)
        {
            using Repository repository = new(Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(fileName1, content1);
            IndexAdd(repository, fileName1);
            if (content2 != null && fileName2 != null)
            {
                _moduleTestHelper.CreateRepoFile(fileName2, content2);
                IndexAdd(repository, fileName2);
            }

            CommitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {CommitHash}, message: {commitMessage}");
            return CommitHash;
        }

        public string CreateRepoFile(string fileName, string fileContent) => _moduleTestHelper.CreateRepoFile(fileName, fileContent);

        public string CreateCommitRelative(string fileRelativePath, string fileName, string commitMessage, string content = null)
        {
            using Repository repository = new(Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(fileRelativePath, fileName, content ?? commitMessage);
            IndexAdd(repository, Path.Combine(fileRelativePath, fileName));

            CommitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {CommitHash}, message: {commitMessage}");

            return CommitHash;
        }

        public string DeleteRepoFile(string fileName) => _moduleTestHelper.DeleteRepoFile(fileName);

        public string RenameRepoFile(string fileRelativePath, string oldFileName, string newFileName, string? newContent = null, string? commitMessage = null)
        {
            using Repository repository = new(Module.WorkingDir);
            newContent ??= File.ReadAllText(Path.Combine(Module.WorkingDir, fileRelativePath, oldFileName));
            DeleteRepoFile(oldFileName);
            CreateRepoFile(newFileName, newContent);

            Commands.Stage(repository, Path.Combine(fileRelativePath, oldFileName));
            Commands.Stage(repository, Path.Combine(fileRelativePath, newFileName));

            CommitHash = Commit(repository, commitMessage);
            Console.WriteLine($"Created commit: {CommitHash}, message: {commitMessage}");

            return CommitHash;
        }

        public void CreateAnnotatedTag(string tagName, string commitHash, string message)
        {
            using Repository repository = new(Module.WorkingDir);
            repository.Tags.Add(tagName, commitHash, GetAuthorSignature(), message);
        }

        public void CreateTag(string tagName, string commitHash, bool allowOverwrite = false)
        {
            using Repository repository = new(Module.WorkingDir);
            repository.Tags.Add(tagName, commitHash, allowOverwrite);
        }

        public void CheckoutRevision()
        {
            using Repository repository = new(Module.WorkingDir);
            Commands.Checkout(repository, CommitHash, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
        }

        public void CheckoutBranch(string branchName)
        {
            using Repository repository = new(Module.WorkingDir);
            Commands.Checkout(repository, branchName, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
        }

        public void CreateRemoteForMasterBranch()
        {
            using Repository repository = new(Module.WorkingDir);
            repository.Network.Remotes.Add("origin", "http://useless.url");
            Remote remote = repository.Network.Remotes["origin"];

            Branch masterBranch = repository.Branches["master"];

            repository.Branches.Update(masterBranch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = masterBranch.CanonicalName);
        }

        public void Fetch(string remoteName)
        {
            using Repository repository = new(Module.WorkingDir);
            Commands.Fetch(repository, remoteName, Array.Empty<string>(), new FetchOptions(), null);
        }

        private void Reset()
        {
            // Undo potential impact from earlier tests
            using (Repository repository = new(Module.WorkingDir))
            {
                CheckoutOptions options = new();
                repository.Reset(LibGit2Sharp.ResetMode.Hard, (Commit)repository.Lookup(CommitHash, LibGit2Sharp.ObjectType.Commit), options);
                repository.RemoveUntrackedFiles();

                string[] remoteNames = repository.Network.Remotes.Select(remote => remote.Name).ToArray();
                foreach (string remoteName in remoteNames)
                {
                    repository.Network.Remotes.Remove(remoteName);
                }

                repository.Config.Set(SettingKeyString.UserName, "author");
                repository.Config.Set(SettingKeyString.UserEmail, "author@mail.com");
            }

            CommitMessageManager commitMessageManager = new(DummyOwner, Module.WorkingDirGitDir, Module.CommitEncoding);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            commitMessageManager.ResetCommitMessageAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public void Stash(string stashMessage, string content = null)
        {
            using Repository repository = new(Module.WorkingDir);
            _moduleTestHelper.CreateRepoFile(_fileName, content ?? stashMessage);
            IndexAdd(repository, _fileName);

            Stash stash = repository.Stashes.Add(GetAuthorSignature(), stashMessage);
            Console.WriteLine($"Created stash: {stash.Index.Sha}, message: {stashMessage}");
        }

        private static Signature GetAuthorSignature() => new(AuthorName, AuthorEmail, DateTimeOffset.Now);

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
