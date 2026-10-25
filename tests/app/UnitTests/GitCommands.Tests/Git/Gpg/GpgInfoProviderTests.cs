using AwesomeAssertions;
using GitCommands.Git.Gpg;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitCommandsTests.Git.Gpg;

public class GpgInfoProviderTests
{
    private IGitGpgController _gitGpgController = null!;
    private GpgInfoProvider _provider = null!;

    [SetUp]
    public void Setup()
    {
        _gitGpgController = Substitute.For<IGitGpgController>();
        _provider = new GpgInfoProvider(_gitGpgController);
    }

    [Test]
    public async Task LoadGpgInfoAsync_should_throw_if_already_cancelled_before_querying_git()
    {
        GitRevision revision = new(ObjectId.Random());
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        OperationCanceledException? caught = null;
        try
        {
            await _provider.LoadGpgInfoAsync(revision, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        caught.Should().NotBeNull();

        // No git commands should have been dispatched for a request that was already stale.
        await _gitGpgController.DidNotReceive().GetRevisionCommitSignatureStatusAsync(Arg.Any<GitRevision>());
        await _gitGpgController.DidNotReceive().GetRevisionTagSignatureStatusAsync(Arg.Any<GitRevision>());
    }

    [Test]
    public async Task LoadGpgInfoAsync_should_throw_when_cancelled_by_a_newer_selection_while_git_calls_are_in_flight()
    {
        // Simulates the FormBrowse race this guards against: a slower, now-stale request
        // (this call) must not overwrite the result of a newer selection. FormBrowse achieves
        // this by cancelling the previous request's CancellationTokenSequence token as soon as
        // a new selection starts, before this in-flight git query completes.
        GitRevision revision = new(ObjectId.Random());
        using CancellationTokenSource cts = new();

        TaskCompletionSource<CommitStatus> commitStatusSource = new();
        TaskCompletionSource<TagStatus> tagStatusSource = new();
        _gitGpgController.GetRevisionCommitSignatureStatusAsync(revision).Returns(commitStatusSource.Task);
        _gitGpgController.GetRevisionTagSignatureStatusAsync(revision).Returns(tagStatusSource.Task);

        Task<GpgInfo?> loadGpgInfoTask = _provider.LoadGpgInfoAsync(revision, cts.Token);

        // A newer selection arrives and cancels this in-flight request's token...
        await cts.CancelAsync();

        // ...but the underlying git calls (already dispatched) complete anyway.
        commitStatusSource.SetResult(CommitStatus.GoodSignature);
        tagStatusSource.SetResult(TagStatus.NoTag);

        OperationCanceledException? caught = null;
        try
        {
            await loadGpgInfoTask;
        }
        catch (OperationCanceledException ex)
        {
            caught = ex;
        }

        caught.Should().NotBeNull();
    }
}
