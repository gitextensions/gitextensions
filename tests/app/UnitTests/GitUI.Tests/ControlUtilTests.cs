using GitUI;

namespace GitUITests;
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
        using Control root = CreateTestHierarchy();

        // child1, child2, child3, grandchild1, grandchild2
        root.FindDescendants().Count().Should().Be(5);
        root.FindDescendants().Distinct().Count().Should().Be(5);
    }

    [Test]
    public void FindDescendantsOfType()
    {
        using Control root = CreateTestHierarchy();

        root.FindDescendantsOfType<TextBox>().Count().Should().Be(1);
        root.FindDescendantsOfType<Button>().Count().Should().Be(1);
        root.FindDescendantsOfType<Label>().Count().Should().Be(0);
    }

    [Test]
    public void FindDescendantsOfTypeWithPredicate()
    {
        using Control root = CreateTestHierarchy();

        Button? foundButton = root.FindDescendantOfType<Button>(t => t.Text == "Click me");
        foundButton.Should().NotBeNull();
        foundButton!.Text.Should().Be("Click me");

        Button? notFoundButton = root.FindDescendantOfType<Button>(t => t.Text == "Non-existent");
        notFoundButton.Should().BeNull();
    }
}
