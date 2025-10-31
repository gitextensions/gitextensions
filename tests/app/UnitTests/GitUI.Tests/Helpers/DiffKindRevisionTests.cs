using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;

namespace GitUITests.Helpers;

[TestFixture]
public class DiffKindRevisionTests
{
    [Test]
    public void DiffKindRevisionTests_error()
    {
        IReadOnlyList<GitRevision> revisions = null;
        ClassicAssert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "null rev");

        revisions = new List<GitRevision> { null };
        ClassicAssert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "1 null rev");

        revisions = new List<GitRevision> { null, null };
        ClassicAssert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "2 null rev");

        ObjectId head = ObjectId.Random();
        revisions = new List<GitRevision> { new(head), null };
        ClassicAssert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out _, out _, out _), "2nd null rev DiffBLocal");
    }

    [Test]
    public void DiffKindRevisionTests_AB_1p()
    {
        ObjectId parent = ObjectId.Random();
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head) { ParentIds = new[] { parent } } };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual(parent.ToString(), firstRevision, "first");
        ClassicAssert.AreEqual(head.ToString(), secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_1h()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual($"{head}^", firstRevision, "first");
        ClassicAssert.AreEqual(head.ToString(), secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual(headParent.ToString(), firstRevision, "first");
        ClassicAssert.AreEqual(head.ToString(), secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AB_null()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head), null };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual("--root", firstRevision, "first");
        ClassicAssert.AreEqual(head.ToString(), secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_1()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual($"{head}^", firstRevision, "first");
        ClassicAssert.AreEqual(null, secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual(headParent.ToString(), firstRevision, "first");
        ClassicAssert.AreEqual(null, secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_AL_3()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head), null };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual("--root", firstRevision, "first");
        ClassicAssert.AreEqual(null, secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_BL_1()
    {
        ObjectId head = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual(head.ToString(), firstRevision, "first");
        ClassicAssert.AreEqual(null, secondRevision, "second");
    }

    [Test]
    public void DiffKindRevisionTests_BL_2()
    {
        ObjectId head = ObjectId.Random();
        ObjectId headParent = ObjectId.Random();
        GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

        ClassicAssert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
        ClassicAssert.AreEqual(head.ToString(), firstRevision, "first");
        ClassicAssert.AreEqual(null, secondRevision, "second");
    }
}
