using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class BuildStatusColumnProvider : ColumnProvider
{
    public BuildStatusColumnProvider()
        : base("Build Status", new GridLength(150), minimumWidth: 16, resizable: true)
    {
        // BuildServerWatcher activates this provider when its integration is ported.
        Column.IsAvailable = false;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = false;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.Classes.Add("revision-build-status-cell");
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        ((TextBlock)control).Text = string.Empty;
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = revision.BuildStatus?.Tooltip ?? revision.BuildStatus?.Description;
        return toolTip is not null;
    }
}
