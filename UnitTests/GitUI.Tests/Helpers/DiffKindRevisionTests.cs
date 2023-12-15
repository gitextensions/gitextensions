using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;

namespace GitUITests.Helpers
{
    [TestFixture]
    public class DiffKindRevisionTests
    {
        [Test]
        public void DiffKindRevisionTests_error()
        {
            IReadOnlyList<GitRevision> revisions = null;
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "null rev");

            revisions = new List<GitRevision> { null };
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "1 null rev");

            revisions = new List<GitRevision> { null, null };
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "2 null rev");

            ObjectId head = ObjectId.Random();
            revisions = new List<GitRevision> { new(head), null };
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out _, out _, out _), "2nd null rev DiffBLocal");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1p()
        {
            ObjectId parent = ObjectId.Random();
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head) { ParentIds = new[] { parent } } };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual(parent.ToString(), firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1h()
        {
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual($"{head}^", firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_2()
        {
            ObjectId head = ObjectId.Random();
            ObjectId headParent = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual(headParent.ToString(), firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_null()
        {
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head), null };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual("--root", firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_1()
        {
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual($"{head}^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_2()
        {
            ObjectId head = ObjectId.Random();
            ObjectId headParent = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual(headParent.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_3()
        {
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head), null };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual("--root", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_1()
        {
            ObjectId head = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual(head.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_2()
        {
            ObjectId head = ObjectId.Random();
            ObjectId headParent = ObjectId.Random();
            GitRevision[] revisions = new[] { new GitRevision(head), new GitRevision(headParent) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out string? firstRevision, out string? secondRevision, out _), "null rev");
            Assert.AreEqual(head.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }
    }
}
