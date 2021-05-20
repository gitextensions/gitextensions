using System;
using System.IO;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class SubmoduleHelpersTest
    {
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

            GitModule testModule = new(root);

            // Submodule name without spaces in the name

            string text = "diff --git a/Externals/conemu-inside b/Externals/conemu-inside\nindex a17ea0c..b5a3d51 160000\n--- a/Externals/conemu-inside\n+++ b/Externals/conemu-inside\n@@ -1 +1 @@\n-Subproject commit a17ea0c8ebe9d8cd7e634ba44559adffe633c11d\n+Subproject commit b5a3d51777c85a9aeee534c382b5ccbb86b485d3\n";
            string fileName = "Externals/conemu-inside";

            var status = SubmoduleHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("b5a3d51777c85a9aeee534c382b5ccbb86b485d3"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("a17ea0c8ebe9d8cd7e634ba44559adffe633c11d"), status.OldCommit);
            Assert.AreEqual(fileName, status.OldName);

            // Submodule name with spaces in the name

            text = "diff --git a/Assets/Core/Vehicle Physics core assets b/Assets/Core/Vehicle Physics core assets\nindex 2fb8851..0cc457d 160000\n--- a/Assets/Core/Vehicle Physics core assets\t\n+++ b/Assets/Core/Vehicle Physics core assets\t\n@@ -1 +1 @@\n-Subproject commit 2fb88514cfdc37a2708c24f71eca71c424b8d402\n+Subproject commit 0cc457d030e92f804569407c7cd39893320f9740\n";
            fileName = "Assets/Core/Vehicle Physics core assets";

            status = SubmoduleHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("0cc457d030e92f804569407c7cd39893320f9740"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("2fb88514cfdc37a2708c24f71eca71c424b8d402"), status.OldCommit);
            Assert.AreEqual(fileName, status.OldName);

            // Submodule name in reverse diff, rename

            text = "diff --git b/Externals/conemu-inside-b a/Externals/conemu-inside-a\nindex a17ea0c..b5a3d51 160000\n--- b/Externals/conemu-inside-b\n+++ a/Externals/conemu-inside-a\n@@ -1 +1 @@\n-Subproject commit a17ea0c8ebe9d8cd7e634ba44559adffe633c11d\n+Subproject commit b5a3d51777c85a9aeee534c382b5ccbb86b485d3\n";
            fileName = "Externals/conemu-inside-b";

            status = SubmoduleHelpers.ParseSubmoduleStatus(text, testModule, fileName);

            Assert.AreEqual(ObjectId.Parse("b5a3d51777c85a9aeee534c382b5ccbb86b485d3"), status.Commit);
            Assert.AreEqual(fileName, status.Name);
            Assert.AreEqual(ObjectId.Parse("a17ea0c8ebe9d8cd7e634ba44559adffe633c11d"), status.OldCommit);
            Assert.AreEqual("Externals/conemu-inside-a", status.OldName);

            text = "diff --git a/Externals/ICSharpCode.TextEditor b/Externals/ICSharpCode.TextEditor\r\nnew file mode 160000\r\nindex 000000000..05321769f\r\n--- /dev/null\r\n+++ b/Externals/ICSharpCode.TextEditor\r\n@@ -0,0 +1 @@\r\n+Subproject commit 05321769f039f39fa7f6748e8f30d5c8f157c7dc\r\n";
            fileName = "Externals/ICSharpCode.TextEditor";

            status = SubmoduleHelpers.ParseSubmoduleStatus(text, testModule, fileName);

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
    }
}
