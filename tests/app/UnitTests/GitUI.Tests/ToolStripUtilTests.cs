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

        ClassicAssert.AreEqual(5, root.FindToolStripItems().Count());
        ClassicAssert.AreEqual(5, root.FindToolStripItems().Distinct().Count());
    }
}
