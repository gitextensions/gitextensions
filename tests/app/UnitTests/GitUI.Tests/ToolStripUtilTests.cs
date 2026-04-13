using GitExtUtils.GitUI;

namespace GitUITests;
public sealed class ToolStripUtilTests
{
    [Test]
    public void FindToolStripItems()
    {
        using ToolStrip root = new()
        {
            Items =
            {
                new ToolStripButton(), new ToolStripButton(),
                new ToolStripDropDownButton { DropDownItems = { new ToolStripButton(), new ToolStripSplitButton() } }
            }
        };

        root.FindToolStripItems().Count().Should().Be(5);
        root.FindToolStripItems().Distinct().Count().Should().Be(5);
    }
}
