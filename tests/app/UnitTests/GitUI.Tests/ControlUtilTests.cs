using GitUI;

namespace GitUITests;

[TestFixture]
public sealed class ControlUtilTests
{
    private static Control CreateTestHierarchy()
    {
        Button grandchild1 = new() { Text = "Click me" };
        Control grandchild2 = new();

        Control child1 = new();
        TextBox child2 = new() { Controls = { grandchild1, grandchild2 } };
        Control child3 = new();

        Control root = new()
        {
            Controls = { child1, child2, child3 }
        };

        return root;
    }

    [Test]
    public void FindDescendants()
    {
        Control root = CreateTestHierarchy();

        // child1, child2, child3, grandchild1, grandchild2
        ClassicAssert.AreEqual(5, root.FindDescendants().Count());
        ClassicAssert.AreEqual(5, root.FindDescendants().Distinct().Count());
    }

    [Test]
    public void FindDescendantsOfType()
    {
        Control root = CreateTestHierarchy();

        ClassicAssert.AreEqual(1, root.FindDescendantsOfType<TextBox>().Count());
        ClassicAssert.AreEqual(1, root.FindDescendantsOfType<Button>().Count());
        ClassicAssert.AreEqual(0, root.FindDescendantsOfType<Label>().Count());
    }

    [Test]
    public void FindDescendantsOfTypeWithPredicate()
    {
        Control root = CreateTestHierarchy();

        Button? foundButton = root.FindDescendantOfType<Button>(t => t.Text == "Click me");
        ClassicAssert.NotNull(foundButton);
        ClassicAssert.AreEqual("Click me", foundButton.Text);

        Button? notFoundButton = root.FindDescendantOfType<Button>(t => t.Text == "Non-existent");
        ClassicAssert.Null(notFoundButton);
    }
}
