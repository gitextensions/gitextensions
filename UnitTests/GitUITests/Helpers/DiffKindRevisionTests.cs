using System.Collections.Generic;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.Helpers
{
    [TestFixture]
    public class DiffKindRevisionTests
    {
        [Test]
        public void DiffKindRevisionTests_error()
        {
            IReadOnlyList<GitRevision> revisions = null;
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _, out _), "null rev");

            revisions = new List<GitRevision> { null };
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _, out _), "1 null rev");

            revisions = new List<GitRevision> { null, null };
            Assert.False(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out _, out _, out _), "2 null rev");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1p()
        {
            var parent = ObjectId.Random();
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) { ParentIds = new[] { parent } } };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual(parent.ToString(), firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1h()
        {
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out var extraDiffArgs, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual("-M -C", extraDiffArgs);
            Assert.AreEqual($"{head}^", firstRevision, "first");
            Assert.AreEqual(head.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_2()
        {
            var head = ObjectId.Random();
            var headParent = ObjectId.Random();
            var revisions = new[] { new GitRevision(headParent), new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAB, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual(head.ToString(), firstRevision, "first");
            Assert.AreEqual(headParent.ToString(), secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_1()
        {
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual($"{head}^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_2()
        {
            var head = ObjectId.Random();
            var headParent = ObjectId.Random();
            var revisions = new[] { new GitRevision(headParent), new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffALocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual(head.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_ApL_1()
        {
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffAParentLocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual($"{head}^^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_1()
        {
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual(head.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_2()
        {
            var head = ObjectId.Random();
            var headParent = ObjectId.Random();
            var revisions = new[] { new GitRevision(headParent), new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBLocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual(headParent.ToString(), firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BpL_1()
        {
            var head = ObjectId.Random();
            var revisions = new[] { new GitRevision(head) };

            Assert.True(RevisionDiffInfoProvider.TryGet(revisions, RevisionDiffKind.DiffBParentLocal, out _, out var firstRevision, out var secondRevision, out _), "null rev");
            Assert.AreEqual($"{head}^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }
    }
}
