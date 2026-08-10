using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI;

partial class FileStatusList
{
    private readonly Dictionary<string, IImage> _fileIconsByExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, MissingIcon> _missingFileIconsByExtension = new(StringComparer.OrdinalIgnoreCase);

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
                image.Source = Images.File;
                if (_missingFileIconsByExtension.TryGetValue(extension, out MissingIcon? missingIcon))
                {
                    missingIcon.Images.Add(image);
                    continue;
                }

                missingIcon = new MissingIcon(fileName, [image]);
                _missingFileIconsByExtension.Add(extension, missingIcon);
                string workingDirectory = TryGetUICommandsDirect(out IGitUICommands? commands)
                    ? commands.Module.WorkingDir
                    : string.Empty;
                ThreadHelper.FileAndForget(async () =>
                {
                    IImage resolved = await Task.Run(
                        () => _iconProvider.Get(workingDirectory, missingIcon.ExampleFileName)?.PlatformIcon as IImage ?? Images.File,
                        cancellationToken);
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _fileIconsByExtension[extension] = resolved;
                        _missingFileIconsByExtension.Remove(extension);
                        foreach (Image pendingImage in missingIcon.Images)
                        {
                            pendingImage.Source = resolved;
                        }
                    });
                });
                continue;
            }

            image.Source = icon;
        }
    }
}
