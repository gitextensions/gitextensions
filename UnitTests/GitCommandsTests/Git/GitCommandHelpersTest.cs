using System;
using System.IO;
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
            // TODO produce a valid working directory
            var module = new GitModule(Path.GetTempPath());
            {
                // Specifying a remote and a local branch creates a local branch
                var fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
                Assert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                var fetchCmd = module.FetchCmd("origin", "some-branch", "local", true).Arguments;
                Assert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --tags", fetchCmd);
            }

            {
                // Using a URL as remote and passing a local branch creates the branch
                var fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", "local").Arguments;
                Assert.AreEqual("fetch --progress \"https://host.com/repo\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                // Using a URL as remote and not passing a local branch
                var fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", null).Arguments;
                Assert.AreEqual("fetch --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // No remote branch -> No local branch
                var fetchCmd = module.FetchCmd("origin", "", "local").Arguments;
                Assert.AreEqual("fetch --progress \"origin\" --no-tags", fetchCmd);
            }

            {
                // Pull doesn't accept a local branch ever
                var fetchCmd = module.PullCmd("origin", "some-branch", false).Arguments;
                Assert.AreEqual("pull --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Not even for URL remote
                var fetchCmd = module.PullCmd("https://host.com/repo", "some-branch", false).Arguments;
                Assert.AreEqual("pull --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Pull with rebase
                var fetchCmd = module.PullCmd("origin", "some-branch", true).Arguments;
                Assert.AreEqual("pull --rebase --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }
        }

        [Test]
        public void TestGetDiffChangedFilesFromString()
        {
            // TODO produce a valid working directory
            var module = new GitModule(Path.GetTempPath());
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
            // TODO produce a valid working directory
            var module = new GitModule(Path.GetTempPath());
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
        public void GetSubmoduleNamesFromDiffTest()
        {
            // TODO produce a valid working directory
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            // Create actual working directories so that Process.Start doesn't throw Win32Exception due to an invalid path
            Directory.CreateDirectory(Path.Combine(root, "Externals", "conemu-inside"));
            Directory.CreateDirectory(Path.Combine(root, "Externals", "conemu-inside-a"));
            Directory.CreateDirectory(Path.Combine(root, "Externals", "conemu-inside-b"));
            Directory.CreateDirectory(Path.Combine(root, "Assets", "Core", "Vehicle Physics core assets"));

            var testModule = new GitModule(root);

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

            try
            {
                // Clean up temporary folders
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Ignore
            }
        }

        [Test]
        public void SubmoduleSyncCmd()
        {
            Assert.AreEqual("submodule sync \"foo\"", GitCommandHelpers.SubmoduleSyncCmd("foo").Arguments);
            Assert.AreEqual("submodule sync", GitCommandHelpers.SubmoduleSyncCmd("").Arguments);
            Assert.AreEqual("submodule sync", GitCommandHelpers.SubmoduleSyncCmd(null).Arguments);
        }

        [Test]
        public void AddSubmoduleCmd()
        {
            Assert.AreEqual(
                "submodule add -b \"branch\" \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", "branch", force: false).Arguments);

            Assert.AreEqual(
                "submodule add \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", branch: null, force: false).Arguments);

            Assert.AreEqual(
                "submodule add -f -b \"branch\" \"remotepath\" \"localpath\"",
                GitCommandHelpers.AddSubmoduleCmd("remotepath", "localpath", "branch", force: true).Arguments);

            Assert.AreEqual(
                "submodule add -f -b \"branch\" \"remote/path\" \"local/path\"",
                GitCommandHelpers.AddSubmoduleCmd("remote\\path", "local\\path", "branch", force: true).Arguments);
        }

        [Test]
        public void RevertCmd()
        {
            var commitId = ObjectId.Random();

            Assert.AreEqual(
                $"revert {commitId}",
                GitCommandHelpers.RevertCmd(commitId, autoCommit: true, parentIndex: 0).Arguments);

            Assert.AreEqual(
                $"revert --no-commit {commitId}",
                GitCommandHelpers.RevertCmd(commitId, autoCommit: false, parentIndex: 0).Arguments);

            Assert.AreEqual(
                $"revert -m 1 {commitId}",
                GitCommandHelpers.RevertCmd(commitId, autoCommit: true, parentIndex: 1).Arguments);
        }

        [Test]
        public void CloneCmd()
        {
            Assert.AreEqual(
                "clone -v --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to").Arguments);
            Assert.AreEqual(
                "clone -v --progress \"from/path\" \"to/path\"",
                GitCommandHelpers.CloneCmd("from\\path", "to\\path").Arguments);
            Assert.AreEqual(
                "clone -v --bare --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", central: true).Arguments);
            Assert.AreEqual(
                "clone -v --recurse-submodules --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", initSubmodules: true).Arguments);
            Assert.AreEqual(
                "clone -v --recurse-submodules --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", initSubmodules: true).Arguments);
            Assert.AreEqual(
                "clone -v --depth 2 --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", depth: 2).Arguments);
            Assert.AreEqual(
                "clone -v --single-branch --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", isSingleBranch: true).Arguments);
            Assert.AreEqual(
                "clone -v --no-single-branch --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", isSingleBranch: false).Arguments);
            Assert.AreEqual(
                "clone -v --progress --branch branch \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", branch: "branch").Arguments);
            Assert.AreEqual(
                "clone -v --progress --no-checkout \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", branch: null).Arguments);
            Assert.AreEqual(
                "lfs clone -v --progress \"from\" \"to\"",
                GitCommandHelpers.CloneCmd("from", "to", lfs: true).Arguments);
        }

        [Test]
        public void CheckoutCmd()
        {
            Assert.AreEqual(
                "checkout \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.DontChange).Arguments);
            Assert.AreEqual(
                "checkout --merge \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Merge).Arguments);
            Assert.AreEqual(
                "checkout --force \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Reset).Arguments);
            Assert.AreEqual(
                "checkout \"branch\"",
                GitCommandHelpers.CheckoutCmd("branch", LocalChangesAction.Stash).Arguments);
        }

        [Test]
        public void RemoveCmd()
        {
            // TODO file names should be quoted

            Assert.AreEqual(
                "rm --force -r .",
                GitCommandHelpers.RemoveCmd().Arguments);
            Assert.AreEqual(
                "rm -r .",
                GitCommandHelpers.RemoveCmd(force: false).Arguments);
            Assert.AreEqual(
                "rm --force .",
                GitCommandHelpers.RemoveCmd(isRecursive: false).Arguments);
            Assert.AreEqual(
                "rm --force -r a b c",
                GitCommandHelpers.RemoveCmd(files: new[] { "a", "b", "c" }).Arguments);
        }

        [Test]
        public void BranchCmd()
        {
            // TODO split this into BranchCmd and CheckoutCmd

            Assert.AreEqual(
                "checkout -b \"branch\" \"revision\"",
                GitCommandHelpers.BranchCmd("branch", "revision", checkout: true).Arguments);
            Assert.AreEqual(
                "branch \"branch\" \"revision\"",
                GitCommandHelpers.BranchCmd("branch", "revision", checkout: false).Arguments);
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", null, checkout: true).Arguments);
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", "", checkout: true).Arguments);
            Assert.AreEqual(
                "checkout -b \"branch\"",
                GitCommandHelpers.BranchCmd("branch", "  ", checkout: true).Arguments);
        }

        [Test]
        public void PushTagCmd()
        {
            // TODO test case where this is false
            Assert.True(GitVersion.Current.PushCanAskForProgress);

            Assert.AreEqual(
                "push --progress \"path\" tag tag",
                GitCommandHelpers.PushTagCmd("path", "tag", all: false).Arguments);
            Assert.AreEqual(
                "push --progress \"path\" tag tag",
                GitCommandHelpers.PushTagCmd("path", " tag ", all: false).Arguments);
            Assert.AreEqual(
                "push --progress \"path/path\" tag tag",
                GitCommandHelpers.PushTagCmd("path\\path", " tag ", all: false).Arguments);
            Assert.AreEqual(
                "push --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true).Arguments);
            Assert.AreEqual(
                "push -f --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true, force: ForcePushOptions.Force).Arguments);
            Assert.AreEqual(
                "push --force-with-lease --progress \"path\" --tags",
                GitCommandHelpers.PushTagCmd("path", "tag", all: true, force: ForcePushOptions.ForceWithLease).Arguments);

            // TODO this should probably throw rather than return an empty string
            Assert.AreEqual(
                "",
                GitCommandHelpers.PushTagCmd("path", "", all: false).Arguments);
        }

        [Test]
        public void StashSaveCmd()
        {
            // TODO test case where message string contains quotes
            // TODO test case where message string contains newlines
            // TODO test case where selectedFiles contains whitespaces (not currently quoted)

            // TODO test case where this is false
            Assert.True(GitVersion.Current.StashUntrackedFilesSupported);

            Assert.AreEqual(
                "stash save",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, Array.Empty<string>()).Arguments);

            Assert.AreEqual(
                "stash save",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, null).Arguments);

            Assert.AreEqual(
                "stash save -u",
                GitCommandHelpers.StashSaveCmd(untracked: true, keepIndex: false, null, null).Arguments);

            Assert.AreEqual(
                "stash save --keep-index",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: true, null, null).Arguments);

            Assert.AreEqual(
                "stash save --keep-index",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: true, null, null).Arguments);

            Assert.AreEqual(
                "stash save \"message\"",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, "message", null).Arguments);

            Assert.AreEqual(
                "stash push -- \"a\" \"b\"",
                GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, new[] { "a", "b" }).Arguments);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void StashSaveCmd_should_not_add_empty_message_full_stash(string theMessage)
        {
            Assert.AreEqual(
               "stash save",
               GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, theMessage, Array.Empty<string>()).Arguments);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void StashPushCmd_should_not_add_empty_message_partial_stash(string theMessage)
        {
            Assert.AreEqual(
               "stash push -- \"a\" \"b\"",
               GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, theMessage, new[] { "a", "b" }).Arguments);
        }

        [Test]
        public void StashSaveCmd_should_add_message_if_provided_full_stash()
        {
            Assert.AreEqual(
               "stash save \"test message\"",
               GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, "test message", Array.Empty<string>()).Arguments);
        }

        [Test]
        public void StashPushCmd_should_add_message_if_provided_partial_stash()
        {
            Assert.AreEqual(
               "stash push -m \"test message\" -- \"a\" \"b\"",
               GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, "test message", new[] { "a", "b" }).Arguments);
        }

        [Test]
        public void StashPushCmd_should_not_add_null_or_empty_filenames()
        {
            Assert.AreEqual(
               "stash push -- \"a\"",
               GitCommandHelpers.StashSaveCmd(untracked: false, keepIndex: false, null, new[] { null, "", "a" }).Arguments);
        }

        [Test]
        public void ContinueBisectCmd()
        {
            var id1 = ObjectId.Random();
            var id2 = ObjectId.Random();

            Assert.AreEqual(
                "bisect good",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good).Arguments);
            Assert.AreEqual(
                "bisect bad",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad).Arguments);
            Assert.AreEqual(
                "bisect skip",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Skip).Arguments);
            Assert.AreEqual(
                $"bisect good {id1} {id2}",
                GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good, id1, id2).Arguments);
        }

        [Test]
        public void RebaseCmd()
        {
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: false).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase -i --no-autosquash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: false, autosquash: false, autoStash: false).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase --preserve-merges \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: true, autosquash: false, autoStash: false).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: true, autoStash: false).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase --autostash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: true).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase -i --autosquash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: false, autosquash: true, autoStash: false).Arguments);
            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase -i --autosquash --preserve-merges --autostash \"branch\"",
                GitCommandHelpers.RebaseCmd("branch", interactive: true, preserveMerges: true, autosquash: true, autoStash: true).Arguments);

            // TODO quote 'onto'?

            Assert.AreEqual(
                "-c rebase.autoSquash=false rebase \"from\" \"branch\" --onto onto",
                GitCommandHelpers.RebaseCmd("branch", interactive: false, preserveMerges: false, autosquash: false, autoStash: false, "from", "onto").Arguments);

            Assert.Throws<ArgumentException>(
                () => GitCommandHelpers.RebaseCmd("branch", false, false, false, false, from: null, onto: "onto"));

            Assert.Throws<ArgumentException>(
                () => GitCommandHelpers.RebaseCmd("branch", false, false, false, false, from: "from", onto: null));
        }

        [TestCase(CleanMode.OnlyNonIgnored, true, false, null, "clean --dry-run")]
        [TestCase(CleanMode.OnlyNonIgnored, false, false, null, "clean -f")]
        [TestCase(CleanMode.OnlyNonIgnored, false, true, null, "clean -d -f")]
        [TestCase(CleanMode.OnlyNonIgnored, false, false, "paths", "clean -f paths")]
        [TestCase(CleanMode.OnlyIgnored, false, false, null, "clean -X -f")]
        [TestCase(CleanMode.All, false, false, null, "clean -x -f")]
        public void CleanCmd(CleanMode mode, bool dryRun, bool directories, string paths, string expected)
        {
            Assert.AreEqual(expected, GitCommandHelpers.CleanCmd(mode, dryRun, directories, paths).Arguments);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void ResetCmd_should_throw_if_ResetIndex_and_hash_is_null_or_empty(string hash)
        {
            Assert.Throws<ArgumentException>(
                () => GitCommandHelpers.ResetCmd(ResetMode.ResetIndex, commit: hash, file: "file.txt"));
        }

        [TestCase(ResetMode.ResetIndex, "tree-ish", null, @"reset ""tree-ish"" --")]
        [TestCase(ResetMode.ResetIndex, "tree-ish", "file.txt", @"reset ""tree-ish"" -- ""file.txt""")]
        [TestCase(ResetMode.Soft, null, null, @"reset --soft --")]
        [TestCase(ResetMode.Mixed, null, null, @"reset --mixed --")]
        [TestCase(ResetMode.Hard, null, null, @"reset --hard --")]
        [TestCase(ResetMode.Soft, "tree-ish", null, @"reset --soft ""tree-ish"" --")]
        [TestCase(ResetMode.Mixed, "tree-ish", null, @"reset --mixed ""tree-ish"" --")]
        [TestCase(ResetMode.Hard, "tree-ish", null, @"reset --hard ""tree-ish"" --")]
        [TestCase(ResetMode.Soft, null, "file.txt", @"reset --soft -- ""file.txt""")]
        [TestCase(ResetMode.Mixed, null, "file.txt", @"reset --mixed -- ""file.txt""")]
        [TestCase(ResetMode.Hard, null, "file.txt", @"reset --hard -- ""file.txt""")]
        [TestCase(ResetMode.Soft, "tree-ish", "file.txt", @"reset --soft ""tree-ish"" -- ""file.txt""")]
        [TestCase(ResetMode.Mixed, "tree-ish", "file.txt", @"reset --mixed ""tree-ish"" -- ""file.txt""")]
        [TestCase(ResetMode.Hard, "tree-ish", "file.txt", @"reset --hard ""tree-ish"" -- ""file.txt""")]
        public void ResetCmd(ResetMode mode, string commit, string file, string expected)
        {
            Assert.AreEqual(expected, GitCommandHelpers.ResetCmd(mode, commit, file).Arguments);
        }

        [Test]
        public void GetAllChangedFilesCmd()
        {
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules --ignored",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: false, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=no --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.No, IgnoreSubmodulesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=normal --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Normal, IgnoreSubmodulesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files=all --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.All, IgnoreSubmodulesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=none",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.None).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=none",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=untracked",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Untracked).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=dirty",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Dirty).Arguments);
            Assert.AreEqual(
                "status --porcelain=2 -z --untracked-files --ignore-submodules=all",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.All).Arguments);
            Assert.AreEqual(
                "--no-optional-locks status --porcelain=2 -z --untracked-files --ignore-submodules",
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.Default, IgnoreSubmodulesMode.Default, noLocks: true).Arguments);
        }

        [Test]
        public void MergeBranchCmd()
        {
            Assert.AreEqual(
                "merge branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --no-ff branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: false, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --squash branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: true, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --no-commit branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: true, allowUnrelatedHistories: false, message: null, log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --allow-unrelated-histories branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: true, message: null, log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge -m \"message\" branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: "message", log: null, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --log=0 branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: 0, strategy: null).Arguments);
            Assert.AreEqual(
                "merge --strategy=strategy branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: true, squash: false, noCommit: false, allowUnrelatedHistories: false, message: null, log: null, strategy: "strategy").Arguments);
            Assert.AreEqual(
                "merge --no-ff --strategy=strategy --squash --no-commit --allow-unrelated-histories -m \"message\" --log=1 branch",
                GitCommandHelpers.MergeBranchCmd("branch", allowFastForward: false, squash: true, noCommit: true, allowUnrelatedHistories: true, message: "message", log: 1, strategy: "strategy").Arguments);
        }

        [Test]
        public void ApplyDiffPatchCmd()
        {
            Assert.AreEqual(
                "apply \"hello/world.patch\"",
                GitCommandHelpers.ApplyDiffPatchCmd(false, "hello\\world.patch").Arguments);
            Assert.AreEqual(
                "apply --ignore-whitespace \"hello/world.patch\"",
                GitCommandHelpers.ApplyDiffPatchCmd(true, "hello\\world.patch").Arguments);
        }

        [Test]
        public void ApplyMailboxPatchCmd()
        {
            Assert.AreEqual(
                "am --3way --signoff \"hello/world.patch\"",
                GitCommandHelpers.ApplyMailboxPatchCmd(false, "hello\\world.patch").Arguments);
            Assert.AreEqual(
                "am --3way --signoff --ignore-whitespace \"hello/world.patch\"",
                GitCommandHelpers.ApplyMailboxPatchCmd(true, "hello\\world.patch").Arguments);
            Assert.AreEqual(
                "am --3way --signoff --ignore-whitespace",
                GitCommandHelpers.ApplyMailboxPatchCmd(true).Arguments);
        }
    }
}
