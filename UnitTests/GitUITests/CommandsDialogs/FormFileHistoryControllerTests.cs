using System;
using System.IO;
using CommonTestUtils;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public sealed class FormFileHistoryControllerTests
    {
        private FormFileHistoryController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new FormFileHistoryController();
        }

        [Test]
        public void TryGetExactPathName()
        {
            // TODO: needs rework/refactor

            var paths = new[]
            {
                @"C:\Users\Public\desktop.ini",
                @"C:\pagefile.sys",
                @"C:\Windows\System32\cmd.exe",
                @"C:\Users\Default\NTUSER.DAT",
                @"C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies",
                @"C:\Program Files (x86)",
                @"Does not exist",
                @"\\" + Environment.MachineName.ToLower() + @"\c$\Windows\System32",
                "",
                " "
            };

            foreach (var path in paths)
            {
                var lowercasePath = path.ToLower();
                var expected = File.Exists(lowercasePath) || Directory.Exists(lowercasePath);
                var actual = _controller.TryGetExactPath(lowercasePath, out string exactPath);

                Assert.AreEqual(expected, actual);

                if (actual)
                {
                    Assert.AreEqual(path.ToLower(), exactPath.ToLower());
                }
                else
                {
                    Assert.IsNull(exactPath);
                }
            }
        }

        [TestCase("Folder1\\file1.txt", true, true)]
        [TestCase("FOLDER1\\file1.txt", true, false)]
        [TestCase("fOLDER1\\file1.txt", true, false)]
        [TestCase("Folder2\\file1.txt", false, false)]
        public void TryGetExactPathName_should_check_if_path_matches_case(string relativePath, bool isResolved, bool doesMatch)
        {
            using (var repo = new GitModuleTestHelper())
            {
                // Create a file
                var notUsed = repo.CreateFile(Path.Combine(repo.TemporaryPath, "Folder1"), "file1.txt", "bla");

                var expected = Path.Combine(repo.TemporaryPath, relativePath);

                Assert.AreEqual(isResolved, _controller.TryGetExactPath(expected, out string exactPath));
                Assert.AreEqual(doesMatch, exactPath == expected);
            }
        }
    }
}