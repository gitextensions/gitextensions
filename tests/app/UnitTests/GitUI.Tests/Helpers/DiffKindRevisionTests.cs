using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;

namespace GitUITests.Helpers;
public class DiffKindRevisionTests
{
    [Test]
    public void DiffKindRevisionTests_error()
    {
        IReadOnlyList<GitRevision>? revisions = null;
        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _).Should().BeFalse("null rev");

        revisions = new List<GitRevision> { null! };
        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _).Should().BeFalse("1 null rev");

        revisions = new List<GitRevision> { null!, null! };
        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _).Should().BeFalse("2 null rev");

        ObjectId head = ObjectId.Random();
        revisions = new List<GitRevision> { new(head), null! };
        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out _, out _, out _).Should().BeFalse("2nd null rev DiffBLocal");
    }

    [Test]
    public void DiffKindRevisionTests_AB_1p()
    {
        ObjectId parent = ObjectId.Random();
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head) { ParentIds = new[] { parent } }];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be(parent.ToString(), "first");
        secondRevision.Should().Be(head.ToString(), "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_1h()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be($"{head}^", "first");
        secondRevision.Should().Be(head.ToString(), "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head), new GitRevision(headParent)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be(headParent.ToString(), "first");
        secondRevision.Should().Be(head.ToString(), "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_null()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head), null!];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be("--root", "first");
        secondRevision.Should().Be(head.ToString(), "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_1()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be($"{head}^", "first");
        secondRevision.Should().Be(null, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head), new GitRevision(headParent)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be(headParent.ToString(), "first");
        secondRevision.Should().Be(null, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_3()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head), null!];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be("--root", "first");
        secondRevision.Should().Be(null, "second");
    }

    [Test]
    public void DiffKindRevisionTests_BL_1()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be(head.ToString(), "first");
        secondRevision.Should().Be(null, "second");
    }

    [Test]
    public void DiffKindRevisionTests_BL_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = [new GitRevision(head), new GitRevision(headParent)];

        RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _).Should().BeTrue("null rev");
        firstRevision.Should().Be(head.ToString(), "first");
        secondRevision.Should().Be(null, "second");
    }
}
