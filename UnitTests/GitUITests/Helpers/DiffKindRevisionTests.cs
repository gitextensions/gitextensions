using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI;
using GitUI.Properties;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using static GitUI.GitUIExtensions;

namespace GitUITests.Helpers
{
    [TestFixture]
    public class DiffKindRevisionTests
    {
        private GitModule _module;

        [SetUp]
        public void Setup()
        {
            _module = new GitModule("");
        }

        [Test]
        public void DiffKindRevisionTests_error()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            revisions = new List<GitRevision> { null };
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "1 null rev");
            revisions = new List<GitRevision> { null, null };
            Assert.AreNotEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "2 null rev");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1p()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            revisions[0].ParentGuids = new string[] { "parent" };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("parent", firstRevision, "first");
            Assert.AreEqual("HEAD", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_1h()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("-M -C", extraDiffArgs);
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual("HEAD", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AB_2()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD^"), new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAB, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual("HEAD^", secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_1()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffALocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_AL_2()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD^"), new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffALocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_ApL_1()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffAParentLocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD^^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_1()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBLocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BL_2()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD^"), new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBLocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }

        [Test]
        public void DiffKindRevisionTests_BpL_1()
        {
            IList<GitRevision> revisions = null;
            string extraDiffArgs, firstRevision, secondRevision;
            revisions = new List<GitRevision> { new GitRevision(_module, "HEAD") };
            Assert.AreEqual("", RevisionDiffInfoProvider.Get(revisions, RevisionDiffKind.DiffBParentLocal, out extraDiffArgs, out firstRevision, out secondRevision), "null rev");
            Assert.AreEqual("HEAD^", firstRevision, "first");
            Assert.AreEqual(null, secondRevision, "second");
        }
    }
}
