using Avalonia.Controls;
using Avalonia.Media;
using GitUI.Properties;

namespace GitUI;

partial class FileStatusList
{
    private readonly Dictionary<string, IImage> _fileIconsByExtension = new(StringComparer.OrdinalIgnoreCase);

    private void LoadFileIcons(IEnumerable<(Image Image, string FileName)> images, CancellationToken cancellationToken)
    {
        foreach ((Image image, string fileName) in images)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            string extension = Path.GetExtension(fileName);
            if (!_fileIconsByExtension.TryGetValue(extension, out IImage? icon))
            {
                // Avalonia has no cross-platform associated-file-icon API. Keep one stable
                // extension cache and use the product's resolved file image on every platform.
                icon = Images.File;
                _fileIconsByExtension[extension] = icon;
            }

            image.Source = icon;
        }
    }
}
