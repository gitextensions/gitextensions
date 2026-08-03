using Avalonia.Controls;

namespace GitUI;

partial class FileStatusList
{
    private sealed class MissingIcon(string exampleFileName, List<Image> images)
    {
        public string ExampleFileName { get; } = exampleFileName;
        public List<Image> Images { get; } = images;
    }
}
