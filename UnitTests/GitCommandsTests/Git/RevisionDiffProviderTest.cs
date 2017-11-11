using GitCommands;
using NUnit.Framework;
using System;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitCommandsTests.Git
{
    [TestClass]
    public class RevisionDiffProviderTest
    {
        [TestMethod]
        public void RevisionDiffProvider_diffoptions()
        {
            //See ArtificialToDiffOptions() for possible "aliases" for artificial commits

#if !DEBUG
            //Testcases that should assert in debug; should not occur but undefined behavior that should be blocked in GUI
            //Cannot run in DEBUG
            //In release build require empty parameters (compare working dir to index)
            Assert.AreEqual("", RevisionDiffProvider.Get(GitRevision.UnstagedGuid, GitRevision.UnstagedGuid).Trim(), "assert 1");
            Assert.AreEqual("", RevisionDiffProvider.Get("", GitRevision.UnstagedGuid).Trim(), "assert 2");
            Assert.AreEqual("", RevisionDiffProvider.Get(null, GitRevision.UnstagedGuid).Trim(), "assert 3");
            Assert.AreEqual("--cached --cached", RevisionDiffProvider.Get(GitRevision.IndexGuid, GitRevision.IndexGuid).Trim(), "assert 4");
#endif

            Assert.AreEqual("", RevisionDiffProvider.Get(GitRevision.IndexGuid, GitRevision.UnstagedGuid).Trim(), "unstaged staged");
            Assert.AreEqual("", RevisionDiffProvider.Get("^", GitRevision.UnstagedGuid).Trim(), "unstaged ^");
            Assert.AreEqual("", RevisionDiffProvider.Get(GitRevision.IndexGuid, "").Trim(), "unstaged empty");
            Assert.AreEqual("\"HEAD\"", RevisionDiffProvider.Get(GitRevision.IndexGuid + "^", GitRevision.UnstagedGuid).Trim(), "unstaged 4");
            Assert.AreEqual("-R", RevisionDiffProvider.Get(GitRevision.UnstagedGuid, GitRevision.IndexGuid).Trim(), "unstaged 5");

            Assert.AreEqual("--cached \"HEAD\"", RevisionDiffProvider.Get("HEAD", GitRevision.IndexGuid).Trim(), "staged 1");
            Assert.AreEqual("-R --cached \"HEAD\"", RevisionDiffProvider.Get(GitRevision.IndexGuid, "HEAD").Trim(), "staged 2");

            Assert.AreEqual("\"HEAD\" \"HEAD\"", RevisionDiffProvider.Get("HEAD", "HEAD").Trim(), "other 1");
            Assert.AreEqual("\"HEAD\" \"123456789\"", RevisionDiffProvider.Get("HEAD", "123456789").Trim(), "other 2");
        }
    }
}

