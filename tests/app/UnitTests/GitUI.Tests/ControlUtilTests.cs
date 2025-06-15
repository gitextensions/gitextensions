using GitUI;

namespace GitUITests
{
    [TestFixture]
    public sealed class ControlUtilTests
    {
        [Test]
        public void FindDescendants()
        {
            Control root = new() { Controls = { new Control(), new Control(), new Control() } };

            ClassicAssert.AreEqual(3, root.FindDescendants().Count());
            ClassicAssert.AreEqual(3, root.FindDescendants().Distinct().Count());
        }

        [Test]
        public void FindDescendantsOfType()
        {
            Control root = new() { Controls = { new Control(), new TextBox(), new Control() } };

            ClassicAssert.AreEqual(1, root.FindDescendantsOfType<TextBox>().Count());
            ClassicAssert.AreEqual(1, root.FindDescendantsOfType<TextBox>().Distinct().Count());
        }

        [Test]
        public void FindDescendantsOfTypeWithPredicate()
        {
            Control root = new() { Controls = { new Control(), new TextBox { Tag = "A" }, new TextBox { Tag = "B" } } };

            ClassicAssert.NotNull(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "A"));
            ClassicAssert.NotNull(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "B"));
            ClassicAssert.Null(root.FindDescendantOfType<TextBox>(t => t.Tag as string == "C"));
        }
    }
}
