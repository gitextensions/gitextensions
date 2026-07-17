using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private readonly RevisionGridControl _grid;

    public MessageColumnProvider(RevisionGridControl grid)
        : base("Message", new GridLength(1, GridUnitType.Star), minimumWidth: 25, resizable: true)
    {
        _grid = grid;
    }

    public override Control CreateCell()
    {
        MessageCell panel = new()
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(ColumnLeftMargin, 0, 2, 0),
            ClipToBounds = true,
        };
        panel.Classes.Add("revision-message-cell");
        panel.Subject.Classes.Add("revision-subject");
        panel.Children.Add(panel.Subject);
        return panel;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        MessageCell panel = (MessageCell)control;
        panel.Children.RemoveRange(0, panel.Children.Count - 1);
        foreach (Control label in RevisionGridRefRenderer.CreateLabels(revision.Refs))
        {
            panel.Children.Insert(panel.Children.Count - 1, label);
        }

        panel.Subject.Text = revision.Subject;
        panel.Subject.FontWeight = _grid.IsCurrentCheckout(revision)
            ? FontWeight.Bold
            : FontWeight.Normal;
        ToolTip.SetTip(panel, AppSettings.ShowRevisionGridTooltips.Value ? revision.Subject : null);
    }

    private sealed class MessageCell : StackPanel
    {
        public TextBlock Subject { get; } = CreateTextBlock();
    }
}
