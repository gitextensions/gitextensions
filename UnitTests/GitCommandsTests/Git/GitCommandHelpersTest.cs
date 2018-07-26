using System;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NUnit.Framework;
using ResourceManager;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitCommandsHelperTest
    {
        [Test]
        public void CanGetRelativeDateString()
        {
            AppSettings.CurrentTranslation = "English";

            var now = DateTime.Now;

            Assert.AreEqual("0 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now));
            Assert.AreEqual("1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-1)));
            Assert.AreEqual("1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-1)));
            Assert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-45)));
            Assert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-1)));
            Assert.AreEqual("1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-1)));
            Assert.AreEqual("1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-7)));
            Assert.AreEqual("1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-30)));
            Assert.AreEqual("12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-364)));
            Assert.AreEqual("1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-365)));

            Assert.AreEqual("2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-2)));
            Assert.AreEqual("2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-2)));
            Assert.AreEqual("2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-2)));
            Assert.AreEqual("2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-2)));
            Assert.AreEqual("2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-14)));
            Assert.AreEqual("2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-60)));
            Assert.AreEqual("2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-730)));
        }

        [Test]
        public void CanGetRelativeNegativeDateString()
        {
            AppSettings.CurrentTranslation = "English";

            var now = DateTime.Now;

            Assert.AreEqual("-1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(1)));
            Assert.AreEqual("-1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(1)));
            Assert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(45)));
            Assert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(1)));
            Assert.AreEqual("-1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(1)));
            Assert.AreEqual("-1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(7)));
            Assert.AreEqual("-1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(30)));
            Assert.AreEqual("-12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(364)));
            Assert.AreEqual("-1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(365)));

            Assert.AreEqual("-2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(2)));
            Assert.AreEqual("-2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(2)));
            Assert.AreEqual("-2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(2)));
            Assert.AreEqual("-2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(2)));
            Assert.AreEqual("-2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(14)));
            Assert.AreEqual("-2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(60)));
            Assert.AreEqual("-2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(730)));
        }

        [Test]
        public void TestFetchArguments()
        {
            var module = new GitModule(null);
            {
                // Specifying a remote and a local branch creates a local branch
                var fetchCmd = module.FetchCmd("origin", "some-branch", "local");
                Assert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                var fetchCmd = module.FetchCmd("origin", "some-branch", "local", true);
                Assert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --tags", fetchCmd);
            }

            {
                // Using a URL as remote and passing a local branch creates the branch
                var fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", "local");
                Assert.AreEqual("fetch --progress \"https://host.com/repo\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                // Using a URL as remote and not passing a local branch
                var fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", null);
                Assert.AreEqual("fetch --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // No remote branch -> No local branch
                var fetchCmd = module.FetchCmd("origin", "", "local");
                Assert.AreEqual("fetch --progress \"origin\" --no-tags", fetchCmd);
            }

            {
                // Pull doesn't accept a local branch ever
                var fetchCmd = module.PullCmd("origin", "some-branch", false);
                Assert.AreEqual("pull --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Not even for URL remote
                var fetchCmd = module.PullCmd("https://host.com/repo", "some-branch", false);
                Assert.AreEqual("pull --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Pull with rebase
                var fetchCmd = module.PullCmd("origin", "some-branch", true);
                Assert.AreEqual("pull --rebase --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }
        }

        [Test]
        public void TestGetDiffChangedFilesFromString()
        {
            var module = new GitModule(null);
            {
                // git diff -M -C -z --cached --name-status
                string statusString = "\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "HEAD", GitRevision.IndexGuid, "HEAD");
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }

            {
                // git diff -M -C -z --cached --name-status
                string statusString = "\0\r\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\r\nThe file will have its original line endings in your working directory.\r\nwarning: LF will be replaced by CRLF in FxCop.targets.\r\nThe file will have its original line endings in your working directory.\r\nM\0testfile.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "HEAD", GitRevision.IndexGuid, "HEAD");
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }

            {
                // git diff -M -C -z --cached --name-status
                string statusString = "\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\nM\0testfile.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "HEAD", GitRevision.IndexGuid, "HEAD");
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }

            {
                // git diff -M -C -z --cached --name-status
                string statusString = "M  testfile.txt\0\nwarning: LF will be replaced by CRLF in CustomDictionary.xml.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in FxCop.targets.\nThe file will have its original line endings in your working directory.\n";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "HEAD", GitRevision.IndexGuid, "HEAD");
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
            }

            {
                // git diff -M -C -z --cached --name-status
                // Ignore unmerged (in conflict) if revision is work tree
                string statusString = "M  testfile.txt\0U  testfile.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, GitRevision.IndexGuid, GitRevision.WorkTreeGuid, GitRevision.IndexGuid);
                Assert.IsTrue(status.Count == 1);
                Assert.IsTrue(status[0].Name == "testfile.txt");
                Assert.IsTrue(status[0].Staged == StagedStatus.WorkTree);
            }

            {
                // git diff -M -C -z --cached --name-status
                // Include unmerged (in conflict) if revision is index
                string statusString = "M  testfile.txt\0U  testfile2.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "HEAD", GitRevision.IndexGuid, "HEAD");
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "testfile.txt");
                Assert.IsTrue(status[0].Staged == StagedStatus.Index);
            }

            {
                // git diff -M -C -z --name-status 123 456
                // Check that the staged status is None if not Index/WorkTree
                string statusString = "M  testfile.txt\0U  testfile2.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, GitRevision.IndexGuid, "456", "678");
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "testfile.txt");
                Assert.IsTrue(status[0].Staged == StagedStatus.None);
            }

            {
                // git diff -M -C -z --name-status 123 456
                // Check that the staged status is None if not Index/WorkTree
                string statusString = "M  testfile.txt\0U  testfile2.txt\0";
                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, "123", "456", null);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "testfile.txt");
                Assert.IsTrue(status[0].Staged == StagedStatus.None);
            }

#if !DEBUG && false
            // This test is for documentation, but as the throw is in a called function, it will not test cleanly
            {
                // git diff -M -C -z --name-status 123 456
                // Check that the staged status is None if not Index/WorkTree
                // Assertion in Debug, throws in Release
                string statusString = "M  testfile.txt\0U  testfile2.txt\0";

                var status = GitCommandHelpers.GetDiffChangedFilesFromString(module, statusString, null, null, null);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "testfile.txt");
                Assert.IsTrue(status[0].Staged == StagedStatus.Unknown);
             }
#endif
        }

        [Test]
        public void TestGetStatusChangedFilesFromString()
        {
            var module = new GitModule(null);
            {
                // git status --porcelain=2 --untracked-files=no -z
                // porcelain v1: string statusString = "M  adfs.h\0M  dir.c\0";
                string statusString = "#Header\03 unknown info\01 .M S..U 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\01 .M SCM. 160000 160000 160000 6bd3b036fc5718a51a0d27cde134c7019798c3ce 6bd3b036fc5718a51a0d27cde134c7019798c3ce dir.c\0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.";
                var status = GitCommandHelpers.GetStatusChangedFilesFromString(module, statusString);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "adfs.h");
                Assert.IsTrue(status[1].Name == "dir.c");
            }

            {
                // git status --porcelain=2 --untracked-files -z
                // porcelain v1: string statusString = "M  adfs.h\0?? untracked_file\0";
                string statusString = "1 .M S..U 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0? untracked_file\0";
                var status = GitCommandHelpers.GetStatusChangedFilesFromString(module, statusString);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "adfs.h");
                Assert.IsTrue(status[1].Name == "untracked_file");
            }

            {
                // git status --porcelain=2 --ignored-files -z
                // porcelain v1: string statusString = ".M  adfs.h\0!! ignored_file\0";
                string statusString = "1 .M S..U 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0! ignored_file\0";
                var status = GitCommandHelpers.GetStatusChangedFilesFromString(module, statusString);
                Assert.IsTrue(status.Count == 2);
                Assert.IsTrue(status[0].Name == "adfs.h");
                Assert.IsTrue(status[1].Name == "ignored_file");
            }
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_Incompatible()
        {
            // Test urls that are incompatible and need to be changed
            string inUrl, expectUrl, outUrl;

            // ssh urls can cause problems
            inUrl = "ssh://user@example.com/path/to/project.git";
            expectUrl = "\"user@example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            inUrl = "ssh://user@example.com:29418/path/to/project.git";
            expectUrl = "-P 29418 \"user@example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);

            // ssh, no user
            inUrl = "ssh://example.com/path/to/project.git";
            expectUrl = "\"example.com:path/to/project.git\"";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual(expectUrl, outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_Compatible()
        {
            // Test urls that are already compatible, these shouldn't be changed
            string inUrl, outUrl;

            // ssh in compatible form
            inUrl = "git@github.com:gitextensions/gitextensions.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // ssh in compatible form, no user
            inUrl = "example.org:some/path/to/repo.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_NoPlink()
        {
            // Test urls that are no valid uris, these should be ignored
            string inUrl, outUrl;

            // git protocol does not have authentication
            inUrl = "git://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // git protocol, different port
            inUrl = "git://server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for http
            inUrl = "http://user@server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, different port
            inUrl = "http://user@server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // http, no user
            inUrl = "http://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // we don't need plink for https
            inUrl = "https://user@server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, different port
            inUrl = "https://user@server:123/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            // https, no user
            inUrl = "https://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [Test]
        public void TestGetPlinkCompatibleUrl_Invalid()
        {
            // Test urls that are no valid uris, these should be ignored
            string inUrl, outUrl;

            inUrl = "foo://server/path/to/project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);

            inUrl = @"ssh:\\server\path\to\project.git";
            outUrl = GitCommandHelpers.GetPlinkCompatibleUrl(inUrl);
            Assert.AreEqual("\"" + inUrl + "\"", outUrl);
        }

        [Test]
        public void GetSubmoduleNamesFromDiffTest()
        {
            var testModule = new GitModule("C:\\Test\\SuperProject");

            // Submodule name without spaces in the name

            string text = "diff --git a/Externals/conemu-inside b/Externals/conemu-inside\nindex a17ea0c..b5a3d51 160000\n--- a/Externals/conemu-inside\n+++ b/Externals/conemu-inside\n@@ -1 +1 @@\n-Subproject commit a17ea0c8ebe9d8cd7e634ba44559adffe633c11d\n+Subproject commit b5a3d51777c85a9aeee534c382b5ccbb86b485d3\n";
            string fileName = "Externals/conemu-inside";

            var status = GitCommandHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("b5a3d51777c85a9aeee534c382b5ccbb86b485d3"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("a17ea0c8ebe9d8cd7e634ba44559adffe633c11d"), status.OldCommit);
            Assert.AreEqual(fileName, status.OldName);

            // Submodule name with spaces in the name

            text = "diff --git a/Assets/Core/Vehicle Physics core assets b/Assets/Core/Vehicle Physics core assets\nindex 2fb8851..0cc457d 160000\n--- a/Assets/Core/Vehicle Physics core assets\t\n+++ b/Assets/Core/Vehicle Physics core assets\t\n@@ -1 +1 @@\n-Subproject commit 2fb88514cfdc37a2708c24f71eca71c424b8d402\n+Subproject commit 0cc457d030e92f804569407c7cd39893320f9740\n";
            fileName = "Assets/Core/Vehicle Physics core assets";

            status = GitCommandHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("0cc457d030e92f804569407c7cd39893320f9740"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("2fb88514cfdc37a2708c24f71eca71c424b8d402"), status.OldCommit);
            Assert.AreEqual(fileName, status.OldName);

            // Submodule name in reverse diff, rename

            text = "diff --git b/Externals/conemu-inside-b a/Externals/conemu-inside-a\nindex a17ea0c..b5a3d51 160000\n--- b/Externals/conemu-inside-b\n+++ a/Externals/conemu-inside-a\n@@ -1 +1 @@\n-Subproject commit a17ea0c8ebe9d8cd7e634ba44559adffe633c11d\n+Subproject commit b5a3d51777c85a9aeee534c382b5ccbb86b485d3\n";
            fileName = "Externals/conemu-inside-b";

            status = GitCommandHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("b5a3d51777c85a9aeee534c382b5ccbb86b485d3"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("a17ea0c8ebe9d8cd7e634ba44559adffe633c11d"), status.OldCommit);
            Assert.AreEqual("Externals/conemu-inside-a", status.OldName);

            text = "diff --git a/Externals/ICSharpCode.TextEditor b/Externals/ICSharpCode.TextEditor\r\nnew file mode 160000\r\nindex 000000000..05321769f\r\n--- /dev/null\r\n+++ b/Externals/ICSharpCode.TextEditor\r\n@@ -0,0 +1 @@\r\n+Subproject commit 05321769f039f39fa7f6748e8f30d5c8f157c7dc\r\n";
            fileName = "Externals/ICSharpCode.TextEditor";

            status = GitCommandHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("05321769f039f39fa7f6748e8f30d5c8f157c7dc"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.IsNull(status.OldCommit);
            Assert.AreEqual("Externals/ICSharpCode.TextEditor", status.OldName);
        }

        [Test]
        public void SubmoduleSyncCmd()
        {
            Assert.AreEqual("submodule sync \"foo\"", GitCommandHelpers.SubmoduleSyncCmd("foo"));
            Assert.AreEqual("submodule sync", GitCommandHelpers.SubmoduleSyncCmd(""));
            Assert.AreEqual("submodule sync", GitCommandHelpers.SubmoduleSyncCmd(null));
        }

        [Test]
        public void AddSubmoduleCmd()
        {
            Assert.AreEqual(
                "submodule add -b \"branch\" \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", "branch", force: false));

            Assert.AreEqual(
                "submodule add \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", branch: null, force: false));

            Assert.AreEqual(
                "submodule add -f -b \"branch\" \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", "branch", force: true));

            Assert.AreEqual(
                "submodule add -f -b \"branch\" \"remote/path\" \"local/path\"",
                GitCommandHelpers.AddSubmoduleCmd("remote\\path", "local\\path", "branch", force: true));
        }

        [Test]
        public void RevertCmd()
        {
            Assert.AreEqual(
                "revert commit",
                GitCommandHelpers.RevertCmd("commit", autoCommit: true, parentIndex: 0));

            Assert.AreEqual(
                "revert --no-commit commit",
                GitCommandHelpers.RevertCmd("commit", autoCommit: false, parentIndex: 0));

            Assert.AreEqual(
                "revert -m 1 commit",
                GitCommandHelpers.RevertCmd("commit", autoCommit: true, parentIndex: 1));
        }

        [Test]
        public void CloneCmd()
        {
            Assert.AreEqual(
                "clone -v --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to"));
            Assert.AreEqual(
                "clone -v --progress \"from/path\" \"to/path\"",
                GitCommandHelpers.CloneCmd("from\\path", "to\\path"));
            Assert.AreEqual(
                "clone -v --bare --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", central: true));
            Assert.AreEqual(
                "clone -v --recurse-submodules --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", initSubmodules: true));
            Assert.AreEqual(
                "clone -v --recurse-submodules --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", initSubmodules: true));
            Assert.AreEqual(
                "clone -v --depth 2 --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", depth: 2));
            Assert.AreEqual(
                "clone -v --single-branch --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", isSingleBranch: true));
            Assert.AreEqual(
                "clone -v --no-single-branch --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", isSingleBranch: false));
            Assert.AreEqual(
                "clone -v --progress --branch branch \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", branch: "branch"));
            Assert.AreEqual(
                "clone -v --progress --no-checkout \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", branch: null));
            Assert.AreEqual(
                "lfs clone -v --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", lfs: true));
        }

        [Test]
        public void CheckoutCmd()
        {
            Assert.AreEqual(
                "checkout \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.DontChange));
            Assert.AreEqual(
                "checkout --merge \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Merge));
            Assert.AreEqual(
                "checkout --force \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Reset));
            Assert.AreEqual(
                "checkout \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Stash));
        }

        [Test]
        public void RemoveCmd()
        {
            // TODO file names should be quoted

            Assert.AreEqual(
                "rm --force -r .",
                GitCommandHelpers.RemoveCmd());
            Assert.AreEqual(
                "rm -r .",
                GitCommandHelpers.RemoveCmd(force: false));
            Assert.AreEqual(
                "rm --force .",
                GitCommandHelpers.RemoveCmd(isRecursive: false));
            Assert.AreEqual(
                "rm --force -r a b c",
                GitCommandHelpers.RemoveCmd(files: new[] { "a", "b", "c" }));
        }

        [Test]
        public void BranchCmd()
        {
            // TODO split this into BranchCmd and CheckoutCmd

            Assert.AreEqual(
                "checkout -b \"branch\" \"revision\"",
                GitCommandHelpers.BranchCmd("branch", "revision", checkout: true));
            Assert.AreEqual(
                "branch \"branch\" \"revision\"",
                GitCommandHelpers.BranchCmd("branch", "revision", checkout: false));
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", null, checkout: true));
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", "", checkout: true));
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", "  ", checkout: true));
        }

        [Test]
        public void PushTagCmd()
        {
            // TODO test case where this is false
            Assert.True(GitCommandHelpers.VersionInUse.PushCanAskForProgress);

            Assert.AreEqual(
                "push --progress \"path\" tag tag",
                GitCommandHelpers.PushTagCmd("path", "tag", all: false));
            Assert.AreEqual(
                "push --progress \"path\" tag tag",
                GitCommandHelpers.PushTagCmd("path", " tag ", all: false));
            Assert.AreEqual(
                "push --progress \"path/path\" tag tag",
                GitCommandHelpers.PushTagCmd("path\\path", " tag ", all: false));
            Assert.AreEqual(
                "push --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true));
            Assert.AreEqual(
                "push -f --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true, force: ForcePushOptions.Force));
            Assert.AreEqual(
                "push --force-with-lease --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true, force: ForcePushOptions.ForceWithLease));

            // TODO this should probably throw rather than return an empty string
            Assert.AreEqual(
                "",
                GitCommandHelpers.PushTagCmd("path", "", all: false));
        }

        [Test]
        public void StashSaveCmd()
        {
            // TODO test case where message string contains quotes
            // TODO test case where message string contains newlines
            // TODO test case where selectedFiles contains whitespaces (not currently quoted)

            // TODO test case where this is false
            Assert.True(GitCommandHelpers.VersionInUse.StashUntrackedFilesSupported);

            Assert.AreEqual(
                "stash save",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, Array.Empty<string>()));

            Assert.AreEqual(
                "stash save",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, null));

            Assert.AreEqual(
                "stash save -u",
                GitCommandHelpers.StashSaveCmd(untracked: true, keepIndex: false, null, null));

            Assert.AreEqual(
                "stash save --keep-index",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: true, null, null));

            Assert.AreEqual(
                "stash save --keep-index",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: true, null, null));

            Assert.AreEqual(
                "stash save \"message\"",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, "message", null));

            Assert.AreEqual(
                "stash push -- a b",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, new[] { "a", "b" }));
        }

        [Test]
        public void ContinueBisectCmd()
        {
            Assert.AreEqual(
                "bisect good",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good));
            Assert.AreEqual(
                "bisect bad",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad));
            Assert.AreEqual(
                "bisect skip",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Skip));
            Assert.AreEqual(
                "bisect good rev1 rev2",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good, "rev1", "rev2"));
        }

        [Test]
        public void RebaseCmd()
        {
            Assert.AreEqual(
                "rebase \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: false));
            Assert.AreEqual(
                "rebase -i --no-autosquash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: false, autosquash: false, autoStash: false));
            Assert.AreEqual(
                "rebase --preserve-merges \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: true, autosquash: false, autoStash: false));
            Assert.AreEqual(
                "rebase \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: true, autoStash: false));
            Assert.AreEqual(
                "rebase --autostash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: true));
            Assert.AreEqual(
                "rebase -i --autosquash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: false, autosquash: true, autoStash: false));
            Assert.AreEqual(
                "rebase -i --autosquash --preserve-merges --autostash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: true, autosquash: true, autoStash: true));

            // TODO quote 'onto'?

            Assert.AreEqual(
                "rebase \"from\" \"branch\" --onto onto",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: false, "from", "onto"));

            Assert.Throws<ArgumentException>(
                () => GitCommandHelpers.RebaseCmd("branch", false, false, false, false, from: null, onto: "onto"));

            Assert.Throws<ArgumentException>(
                () => GitCommandHelpers.RebaseCmd("branch", false, false, false, false, from: "from", onto: null));
        }

        [Test]
        public void CleanUpCmd()
        {
            Assert.AreEqual(
                "clean -f",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: false, nonIgnored: true, ignored: false));
            Assert.AreEqual(
                "clean --dry-run",
                GitCommandHelpers.CleanUpCmd(dryRun: true, directories: false, nonIgnored: true, ignored: false));
            Assert.AreEqual(
                "clean -d -f",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: true, nonIgnored: true, ignored: false));
            Assert.AreEqual(
                "clean -x -f",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: false, nonIgnored: false, ignored: false));
            Assert.AreEqual(
                "clean -X -f",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: false, nonIgnored: true, ignored: true));
            Assert.AreEqual(
                "clean -X -f",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: false, nonIgnored: false, ignored: true));
            Assert.AreEqual(
                "clean -f paths",
                GitCommandHelpers.CleanUpCmd(dryRun: false, directories: false, nonIgnored: true, ignored: false, "paths"));
        }

        [Test]
        public void GetAllChangedFilesCmd()
        {
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules --ignored",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: false, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=no --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.No, IgnoreSubmodulesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=normal --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Normal, IgnoreSubmodulesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=all --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.All, IgnoreSubmodulesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=none",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.None));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=none",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=untracked",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Untracked));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=dirty",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Dirty));
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=all",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.All));
            Assert.AreEqual(
                "--no-optional-locks status --porcelain=2 -z --untracked-files --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default, noLocks: true));
        }

        [Test]
        public void MergeBranchCmd()
        {
            Assert.AreEqual(
                "merge branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null));
            Assert.AreEqual(
                "merge --no-ff branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: false, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null));
            Assert.AreEqual(
                "merge --squash branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: true, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null));
            Assert.AreEqual(
                "merge --no-commit branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: true, allowUnrelatedHistories: false, message: null, log: null, strategy: null));
            Assert.AreEqual(
                "merge --allow-unrelated-histories branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: true, message: null, log: null, strategy: null));
            Assert.AreEqual(
                "merge -m \"message\" branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: "message", log: null, strategy: null));
            Assert.AreEqual(
                "merge --log=0 branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: 0, strategy: null));
            Assert.AreEqual(
                "merge --strategy=strategy branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: "strategy"));
            Assert.AreEqual(
                "merge --no-ff --strategy=strategy --squash --no-commit --allow-unrelated-histories -m \"message\" --log=1 branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: false, squash: true, noCommit: true, allowUnrelatedHistories: true, message: "message", log: 1, strategy: "strategy"));
        }

        [Test]
        public void ApplyDiffPatchCmd()
        {
            Assert.AreEqual(
                "apply \"hello/world.patch\"",
                GitCommandHelpers.ApplyDiffPatchCmd(false, "hello\\world.patch"));
            Assert.AreEqual(
                "apply --ignore-whitespace \"hello/world.patch\"",
                GitCommandHelpers.ApplyDiffPatchCmd(true, "hello\\world.patch"));
        }

        [Test]
        public void ApplyMailboxPatchCmd()
        {
            Assert.AreEqual(
                "am --3way --signoff \"hello/world.patch\"",
                GitCommandHelpers.ApplyMailboxPatchCmd(false, "hello\\world.patch"));
            Assert.AreEqual(
                "am --3way --signoff --ignore-whitespace \"hello/world.patch\"",
                GitCommandHelpers.ApplyMailboxPatchCmd(true, "hello\\world.patch"));
            Assert.AreEqual(
                "am --3way --signoff --ignore-whitespace",
                GitCommandHelpers.ApplyMailboxPatchCmd(true));
        }
    }
}
