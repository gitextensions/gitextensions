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
using System.IO;
using PatchApply;

namespace GitCommandsTests
{
    [TestClass]
    public class PatchTest
    {
        private static void CreateTestFile(TestContext context, string filename, string text)
        {
            using(StreamWriter writer = new StreamWriter(context.TestRunDirectory + filename))
            {
                writer.WriteLine(text);
                writer.Flush();
            }
        }

        private static Patch GetNewPatch()
        {
            return new Patch();    
        }

        [TestMethod]
        public void TestConstructor()
        {
            Patch patch = new Patch();
            Assert.IsNotNull(patch);
        }

        [TestMethod]
        public void TestAppendText()
        {
            Patch patch = GetNewPatch();
            patch.AppendText("text1");
            patch.AppendText("text2");

            Assert.AreEqual(patch.Text, "text1text2");
        }

        [TestMethod]
        public void TestLoadFile()
        {
            Patch patch = GetNewPatch();
        }
    }
}
