#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using GitCommands;

namespace GitCommandsTests
{
    /// <summary>

    /// </summary>
    [TestClass]
    public class FindValidworkingDirTest
    {
        private static string GetCurrentDir()
        {
            string path = typeof(FindValidworkingDirTest).Assembly.Location;

            return path.Substring(0, path.LastIndexOf('\\'));
        }


        [TestMethod]
        public void TestWorkingDir()
        {
            GitModule.CurrentWorkingDir = GetCurrentDir();
            CheckWorkingDir();
            GitModule.CurrentWorkingDir = GetCurrentDir() + "\\testfile.txt";
            CheckWorkingDir();
            GitModule.CurrentWorkingDir = GetCurrentDir() + "\\";
            CheckWorkingDir();
            GitModule.CurrentWorkingDir = GetCurrentDir() + "\\\\";
            CheckWorkingDir();
            GitModule.CurrentWorkingDir = GetCurrentDir() + "\\test\\test\\tralala";
            CheckWorkingDir();

        }

        private static void CheckWorkingDir()
        {
            //Should not contain double slashes -> \\
            Assert.IsFalse(GitModule.CurrentWorkingDir.Contains("\\\\"), "WorkingDir" + GitModule.CurrentWorkingDir + "\n" + GetCurrentDir());

            //Should end with slash
            Assert.IsTrue(GitModule.CurrentWorkingDir.EndsWith("\\"), "WorkingDir" + GitModule.CurrentWorkingDir + "\n" + GetCurrentDir());
        }
    }
}
