using GitExtUtils.GitUI;

namespace GitUITests;

[TestFixture]
public sealed class ToolStripUtilTests
{
    [Test]
    public void FindToolStripItems()
    {
        ToolStrip root = new()
        {
            Items =
            {
                new ToolStripButton(), new ToolStripButton(),
                new ToolStripDropDownButton { DropDownItems = { new ToolStripButton(), new ToolStripSplitButton() } }
            }
        };

        Assert.AreEqual(5, root.FindToolStripItems().Count());
        Assert.AreEqual(5, root.FindToolStripItems().Distinct().Count());
    }
}
