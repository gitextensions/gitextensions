using System.Diagnostics;
using GitCommands;
using GitUI;
using LibGit2Sharp;
using Microsoft.VisualStudio.Threading;

namespace CommonTestUtils;

public class ReferenceRepository : IDisposable
{
    static ReferenceRepository()
    {
        Epilogue.RegisterAfterSuiteAction(1, ReleaseNextRepositories);
    }

    private static readonly Lock _nextLock = new();
    private static Task<(GitModuleTestHelper Helper, string? CommitHash)> _nextGitModuleTestHelper = null;
    private static Task<(GitModuleTestHelper Helper, string? CommitHash)> _nextGitModuleTestHelperWithCommit = null;

    public const string AuthorName = "GitUITests";
    public const string AuthorEmail = "unittests@gitextensions.com";
    public const string AuthorFullIdentity = $"{AuthorName} <{AuthorEmail}>";
    private readonly GitModuleTestHelper _moduleTestHelper;

    // We don't expect any failures so that we won't be switching to the main thread or showing messages
    public static Control DummyOwner { get; } = new();

    public ReferenceRepository(bool createCommit = true)
    {
        lock (_nextLock)
        {
            if (createCommit)
            {
                _moduleTestHelper = ClaimNextModuleTestHelper(ref _nextGitModuleTestHelperWithCommit, createCommit: true);
            }
            else
            {
                _moduleTestHelper = ClaimNextModuleTestHelper(ref _nextGitModuleTestHelper, createCommit: false);
            }
        }
    }

    private GitModuleTestHelper ClaimNextModuleTestHelper(ref Task<(GitModuleTestHelper Helper, string? CommitHash)> initializerRef, bool createCommit)
    {
        // If ReleaseNextRepositories was called, then the pending instances have
        // been cancelled.
        initializerRef ??= BeginInitializeGitModuleTestHelper(createCommit);

        Task<(GitModuleTestHelper Helper, string? CommitHash)> initializer = initializerRef;

        // VSTHRD002 warns that naively accessing the result of a task with affinity to the
        // UI thread when running on the UI thread will result in a deadlock, because it
        // can't advance the task to get that result without the thread running and the call
        // is explicitly blocking it. This is not the UI thread, and the await is specifically
        // configured to not require continuation on captured context. Furthermore, the task
        // was created earlier on this same thread on which there is no synchronization
        // context to begin with.
        GitModuleTestHelper moduleTestHelper;

#pragma warning disable VSTHRD002
        (moduleTestHelper, CommitHash) = initializer.ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        // Start background initialization of the Git module for the next call.
        initializerRef = BeginInitializeGitModuleTestHelper(createCommit);

        return moduleTestHelper;
    }

    private static Task<(GitModuleTestHelper Helper, string? CommitHash)> BeginInitializeGitModuleTestHelper(bool createCommit)
    {
        return Task.Run(
            () =>
            {
                GitModuleTestHelper moduleTestHelper = new();
                string? commitHash = null;

                if (createCommit)
                {
                    commitHash = CreateCommit(moduleTestHelper, "A commit message", "A");
                }

                return (moduleTestHelper, commitHash);
            });
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
        CommitHash = CreateCommit(_moduleTestHelper, commitMessage, content);

        return CommitHash;
    }

    private static string CreateCommit(GitModuleTestHelper moduleTestHelper, string commitMessage, string content = null)
    {
        using Repository repository = new(moduleTestHelper.Module.WorkingDir);
        moduleTestHelper.CreateRepoFile(_fileName, content ?? commitMessage);
        IndexAdd(repository, _fileName);

        string commitHash = Commit(repository, commitMessage);
        Console.WriteLine($"Created commit: {commitHash}, message: {commitMessage}");
        return commitHash;
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

        Module.InvalidateGitSettings();
        Module.GetEffectiveSetting("reload now");
        Module.GetSettings("reload local settings, too");
    }

    public void Fetch(string remoteName)
    {
        using Repository repository = new(Module.WorkingDir);
        Commands.Fetch(repository, remoteName, Array.Empty<string>(), new FetchOptions(), null);
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

    public static void ReleaseNextRepositories()
    {
        Task<(GitModuleTestHelper Helper, string? CommitHash)> next = null;
        Task<(GitModuleTestHelper Helper, string? CommitHash)> nextWithCommit = null;

        lock (_nextLock)
        {
            if (_nextGitModuleTestHelper != null)
            {
                next = _nextGitModuleTestHelper;
                _nextGitModuleTestHelper = null;
            }

            if (_nextGitModuleTestHelperWithCommit != null)
            {
                nextWithCommit = _nextGitModuleTestHelperWithCommit;
                _nextGitModuleTestHelperWithCommit = null;
            }
        }

        TaskManager mgr = new(new JoinableTaskContext());

        // VSTHRD003 says we shouldn't await tasks that came from other
        // places because we don't know anything about them and it might
        // cause deadlocks. These tasks didn't come from other places,
        // though.
        //
        // VSTHRD104 says we should rewrite ReleaseNextRepositories as an
        // async method and then expose a synchronous version that just
        // calls JoinableTaskFactory.Run(ReleaseNextRepositoriesAsync).
        // I tried that, and it just moves VSTHRD104 into the new sync
        // wrapper.
#pragma warning disable VSTHRD003, VSTHRD104
        mgr.JoinableTaskFactory.Run(
            async () =>
            {
                if (next != null)
                {
                    (await next.ConfigureAwait(false)).Helper.Dispose();
                }

                if (nextWithCommit != null)
                {
                    (await nextWithCommit.ConfigureAwait(false)).Helper.Dispose();
                }
            });
#pragma warning restore VSTHRD003, VSTHRD104
    }

    public static void WaitForCleanUpCompletion()
    {
        GitModuleTestHelper.WaitForCleanUpCompletion();
    }
}
