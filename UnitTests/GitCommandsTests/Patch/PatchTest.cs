using NUnit.Framework;
using PatchApply;

namespace GitCommandsTests.PatchApply
{
    [TestFixture]
    internal class PatchTest
    {
        [Test]
        public void TestPatchConstructor()
        {
            Patch patch = new Patch();

            Assert.IsNotNull(patch);
        }

        [Test]
        public void TestAppendText()
        {
            Patch patch = new Patch();
            patch.AppendText("text1");
            patch.AppendText("text2");

            Assert.AreEqual("text1text2", patch.Text);
        }

        [Test]
        public void TestAppendTextLine()
        {
            Patch patch = new Patch();

            patch.AppendTextLine("text1");
            patch.AppendTextLine("text2");
            patch.AppendTextLine("text3");

            var expected = "text1\ntext2\ntext3\n";

            Assert.AreEqual(expected, patch.Text);
        }
    }
}
