using NUnit.Framework;

namespace GitCommandsTests.Patch
{
    [TestFixture]
    internal class PatchTest
    {
        [Test]
        public void TestPatchConstructor()
        {
            PatchApply.Patch patch = new PatchApply.Patch();

            Assert.IsNotNull(patch);
        }

        [Test]
        public void TestAppendText()
        {
            PatchApply.Patch patch = new PatchApply.Patch();
            patch.AppendText("text1");
            patch.AppendText("text2");

            Assert.AreEqual("text1text2", patch.Text);
        }

        [Test]
        public void TestAppendTextLine()
        {
            PatchApply.Patch patch = new PatchApply.Patch();

            patch.AppendTextLine("text1");
            patch.AppendTextLine("text2");
            patch.AppendTextLine("text3");

            var expected = "text1\ntext2\ntext3\n";

            Assert.AreEqual(expected, patch.Text);
        }
    }
}
