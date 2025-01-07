#nullable enable

namespace GitUI;

partial class FileStatusList
{
    private sealed class MissingIcon(string exampleFileName, List<TreeNode> nodes)
    {
        public string ExampleFileName { get; } = exampleFileName;
        public List<TreeNode> Nodes { get; } = nodes;
        public int RelativeImageIndex { get; set; } = -1;
    }
}
