using System.Collections.Generic;
using GitCommands;
using GitUI;
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
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "null rev");

            revisions = new List<GitRevision> { null };
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "1 null rev");

            revisions = new List<GitRevision> { null, null };
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out _, out _, out _), "2 null rev");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1p()
        {
            var revisions = new[] { new GitRevision("HEAD") { ParentGuids = new[] { "parent" } } };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("parent", firstRevision, "first");
            Assert.AreEqual("HEAD", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1h()
        {
            var revisions = new[] { new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out var extraDiffArgs, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("-M -C", extraDiffArgs);
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual("HEAD", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_2()
        {
            var revisions = new[] { new GitRevision("HEAD^"), new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual("HEAD^", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_1()
        {
            var revisions = new[] { new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffALocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_2()
        {
            var revisions = new[] { new GitRevision("HEAD^"), new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffALocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_ApL_1()
        {
            var revisions = new[] { new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAParentLocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD^^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_1()
        {
            var revisions = new[] { new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBLocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_2()
        {
            var revisions = new[] { new GitRevision("HEAD^"), new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBLocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BpL_1()
        {
            var revisions = new[] { new GitRevision("HEAD") };

            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBParentLocal, out _, out var firstRevision, out var secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }
    }
}
