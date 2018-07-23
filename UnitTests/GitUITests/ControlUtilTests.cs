using System.Linq;
using System.Windows.Forms;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public sealed class ControlUtilTests
    {
        [Test]
        public void FindDescendants()
        {
            var root = new Control { Controls = { new Control(), new Control(), new Control() } };

            Assert.AreEqual(3, root.FindDescendants().Count());
            Assert.AreEqual(3, root.FindDescendants().Distinct().Count());
        }

        [Test]
        public void FindDescendantsOfType()
        {
            var root = new Control { Controls = { new Control(), new TextBox(), new Control() } };

            Assert.AreEqual(1, root.FindDescendantsOfType<TextBox>().Count());
            Assert.AreEqual(1, root.FindDescendantsOfType<TextBox>().Distinct().Count());
        }

        [Test]
        public void FindDescendantsOfTypeWithPredicate()
        {
            var root = new Control { Controls = { new Control(), new TextBox { Tag = "A" }, new TextBox { Tag = "B" } } };

            Assert.NotNull(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "A"));
            Assert.NotNull(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "B"));
            Assert.Null(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "C"));
        }
    }
}