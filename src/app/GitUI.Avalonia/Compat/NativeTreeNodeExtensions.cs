using Avalonia.Controls;
using Avalonia.Media;
using GitUI.LeftPanel;

namespace GitUI.Compat;

/// <summary>
///  Gives a repository tree item the text and image of a native tree node, both shown in the item's header.
/// </summary>
internal static class NativeTreeNodeExtensions
{
    /// <summary>Gets the text of the item, like the native node's <c>Text</c>.</summary>
    internal static string GetText(this TreeViewItem treeNode)
        => GetLabel(treeNode)?.Text ?? string.Empty;

    /// <summary>Sets the text of the item, like the native node's <c>Text</c>.</summary>
    internal static void SetText(this TreeViewItem treeNode, string text)
    {
        if (GetLabel(treeNode) is { } label)
        {
            label.Text = text;
            return;
        }

        treeNode.Header = RepoObjectsTree.CreateHeader(text, icon: null);
    }

    /// <summary>
    ///  Sets the image of the item, like the native node's <c>ImageKey</c> and <c>SelectedImageKey</c> together.
    /// </summary>
    internal static void SetImage(this TreeViewItem treeNode, IImage image)
    {
        if (treeNode.Header is not StackPanel { Children: [Image current, ..] } header)
        {
            treeNode.Header = RepoObjectsTree.CreateHeader(string.Empty, image);
            return;
        }

        if (current.Source is { } source
            && ReferenceEquals(NativeTreeImageListImage.Create(source), NativeTreeImageListImage.Create(image)))
        {
            return;
        }

        header.Children[0] = CreateImage(image);
    }

    /// <summary>Creates the image slot of a header; a node without an image keeps the empty slot.</summary>
    internal static Image CreateImage(IImage? image)
        => new NativeTreeImageListControl(image)
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
        };

    private static TextBlock? GetLabel(TreeViewItem treeNode)
        => treeNode.Header is StackPanel header ? header.Children.OfType<TextBlock>().FirstOrDefault() : null;
}
