using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;

using ResourceManager;

namespace GitUI;

// TODO(avalonia-port): milestone M1.3 — a read-only file list for the selected revision.
// The grouping, filtering, staging interactions, and context menus of the WinForms
// FileStatusList (2,198 LOC) arrive in later milestones.
public partial class FileStatusList : GitExtensionsControl
{
    public FileStatusList()
    {
        InitializeComponent();

        lstFiles.ItemTemplate = new FuncDataTemplate<GitItemStatus>(CreateFileRow, supportsRecycling: false);
        lstFiles.SelectionChanged += (_, _) => SelectedIndexChanged?.Invoke(this, EventArgs.Empty);

        InitializeComplete();
    }

    /// <summary>
    ///  Occurs when the selected file changes (named like the WinForms event).
    /// </summary>
    public event EventHandler? SelectedIndexChanged;

    /// <summary>
    ///  Gets the selected file status item, or <see langword="null"/>.
    /// </summary>
    public GitItemStatus? SelectedItem => lstFiles.SelectedItem as GitItemStatus;

    /// <summary>
    ///  Shows the given diff items.
    /// </summary>
    public void SetDiffs(IReadOnlyList<GitItemStatus> items)
    {
        lstFiles.ItemsSource = items;
        lblCount.Text = $"{items.Count} {(items.Count == 1 ? "file" : "files")}";

        // Like the WinForms FileStatusList, select the first file automatically.
        if (items.Count > 0)
        {
            lstFiles.SelectedIndex = 0;
        }
    }

    public void Clear()
    {
        lstFiles.ItemsSource = null;
        lblCount.Text = string.Empty;
    }

    private static Control CreateFileRow(GitItemStatus item, INameScope nameScope)
    {
        (string marker, Avalonia.Media.Color color) = item switch
        {
            { IsNew: true } => ("A", Colors.SeaGreen),
            { IsDeleted: true } => ("D", Colors.IndianRed),
            { IsRenamed: true } => ("R", Colors.SteelBlue),
            _ => ("M", Colors.Goldenrod),
        };

        return new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                new TextBlock
                {
                    Text = marker,
                    Foreground = new SolidColorBrush(color),
                    FontWeight = FontWeight.Bold,
                    Width = 18,
                    Margin = new Avalonia.Thickness(4, 0, 0, 0),
                },
                new TextBlock
                {
                    Text = item.Name,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
            },
        };
    }
}
