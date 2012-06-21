using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PatchApply;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
