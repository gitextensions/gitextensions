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
            CheckWorkingDir(GetCurrentDir());
            CheckWorkingDir(GetCurrentDir() + "\\testfile.txt");
            CheckWorkingDir(GetCurrentDir() + "\\");
            CheckWorkingDir(GetCurrentDir() + "\\\\");
            CheckWorkingDir(GetCurrentDir() + "\\test\\test\\tralala");
        }

        private static void CheckWorkingDir(string path)
        {
            GitModule module = new GitModule(path);
            //Should not contain double slashes -> \\
            Assert.IsFalse(module.WorkingDir.Contains("\\\\"), "WorkingDir" + module.WorkingDir + "\n" + GetCurrentDir());

            //Should end with slash
            Assert.IsTrue(module.WorkingDir.EndsWith("\\"), "WorkingDir" + module.WorkingDir + "\n" + GetCurrentDir());
        }
    }
}
