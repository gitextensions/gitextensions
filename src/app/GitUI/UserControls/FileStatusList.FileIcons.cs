#nullable enable

using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    private void LoadFileIcons(IEnumerable<TreeNode> nodes, CancellationToken cancellationToken)
    {
        Dictionary<string, MissingIcon> missingIconsByExtension = FindMissingIcons(nodes);
        if (missingIconsByExtension.Count > 0 && Module?.WorkingDir is string workingDir)
        {
            ThreadHelper.FileAndForget(() => LoadFileIconsAsync(missingIconsByExtension, fileName => _iconProvider.Get(workingDir, fileName), _imageListData.ImageList.ImageSize, FileStatusListView, cancellationToken));
        }

        return;

        static async Task LoadFileIconsAsync(Dictionary<string, MissingIcon> missingIconsByExtension, Func<string, Icon?> loadFileIcon, Size size, Control control, CancellationToken cancellationToken)
        {
            List<Image> icons = new(capacity: missingIconsByExtension.Count);
            foreach (KeyValuePair<string, MissingIcon> missingIcon in missingIconsByExtension)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                missingIcon.Value.RelativeImageIndex = icons.Count;
                icons.Add(loadFileIcon(missingIcon.Value.ExampleFileName) is Icon icon
                    ? Scale(icon.ToBitmap(), _imageListData.IconSize, size, offsetY: 0)
                    : _imageListData.DefaultFileImage);
            }

            await control.SwitchToMainThreadAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            int imageIndexOffset = _imageListData.ImageList.Images.Count;
            _imageListData.ImageList.Images.AddRange([.. icons]);
            foreach (KeyValuePair<string, MissingIcon> missingIcon in missingIconsByExtension)
            {
                _imageListData.StateImageIndexMap[missingIcon.Key] = missingIcon.Value.RelativeImageIndex + imageIndexOffset;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            int delayCounter = 0;
            foreach (KeyValuePair<string, MissingIcon> missingIcon in missingIconsByExtension)
            {
                int imageIndex = missingIcon.Value.RelativeImageIndex + imageIndexOffset;
                foreach (TreeNode node in missingIcon.Value.Nodes)
                {
                    node.ImageIndex = imageIndex;
                    node.SelectedImageIndex = imageIndex;

                    if (++delayCounter >= 100)
                    {
                        delayCounter = 0;
                        await Task.Delay(millisecondsDelay: 1, cancellationToken);
                    }
                }
            }

            control.Invalidate();
        }

        static Dictionary<string, MissingIcon> FindMissingIcons(IEnumerable<TreeNode> nodes)
        {
            Dictionary<string, MissingIcon> missingIconsByExtension = [];

            int defaultFileImageIndex = _imageListData.StateImageIndexMap[nameof(ImageListData.DefaultFileImage)];
            foreach (TreeNode node in nodes)
            {
                if (node.ImageIndex != defaultFileImageIndex)
                {
                    continue;
                }

                TreeNode fileNode = node.Tag is GroupKey
                    ? node.Items().First(node => node.Nodes.Count == 0)
                    : node;
                if (fileNode.Tag is not FileStatusItem fileStatusItem)
                {
                    continue;
                }

                string fileName = fileStatusItem.Item.Name;
                string extension = Path.GetExtension(fileName);
                if (_imageListData.StateImageIndexMap.TryGetValue(extension, out int imageIndex))
                {
                    node.ImageIndex = imageIndex;
                    node.SelectedImageIndex = imageIndex;
                }
                else if (missingIconsByExtension.TryGetValue(extension, out MissingIcon? entry))
                {
                    entry.Nodes.Add(node);
                }
                else
                {
                    missingIconsByExtension[extension] = new MissingIcon(fileName, [node]);
                }
            }

            return missingIconsByExtension;
        }
    }
}
