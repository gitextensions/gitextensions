using GitUI.LeftPanel;

namespace GitUITests.LeftPanel;

// A non-folder leaf node used in tests to verify compaction stops at non-SubmoduleFolderNode children.
internal sealed class DummyNode : Node
{
    public DummyNode() : base(null)
    {
    }
}
