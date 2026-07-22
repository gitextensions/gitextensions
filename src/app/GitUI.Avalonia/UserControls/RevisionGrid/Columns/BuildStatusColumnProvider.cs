using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class BuildStatusColumnProvider : ColumnProvider
{
    private const int IconColumnWidth = 16;
    private const int TextColumnWidth = 150;
    private readonly Action<GitRevision> _openBuildReport;

    public BuildStatusColumnProvider(Action<GitRevision> openBuildReport)
        : base("Build Status", new GridLength(150), minimumWidth: 16, resizable: true)
    {
        _openBuildReport = openBuildReport;
        Column.IsAvailable = false;
    }

    public override void ApplySettings()
    {
        bool showIcon = AppSettings.ShowBuildStatusIconColumn;
        bool showText = AppSettings.ShowBuildStatusTextColumn;
        Column.IsVisible = showIcon || showText;
        Column.Resizable = showText;
        if (showIcon && !showText)
        {
            Column.Width = new GridLength(IconColumnWidth);
        }
        else if (showText && Column.Width.Value == IconColumnWidth)
        {
            Column.Width = new GridLength(TextColumnWidth);
        }
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.Classes.Add("revision-build-status-cell");
        textBlock.Tapped += (_, _) =>
        {
            if (textBlock.DataContext is GitRevision { BuildStatus.Url: not null } revision)
            {
                _openBuildReport(revision);
            }
        };
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        BuildInfo? buildStatus = revision.BuildStatus;
        textBlock.Text = buildStatus is null
            ? string.Empty
            : (AppSettings.ShowBuildStatusIconColumn ? buildStatus.StatusSymbol : string.Empty)
                + (AppSettings.ShowBuildStatusTextColumn ? buildStatus.Description : string.Empty);
        textBlock.Cursor = string.IsNullOrWhiteSpace(buildStatus?.Url) ? Cursor.Default : new Cursor(StandardCursorType.Hand);
        textBlock.Foreground = buildStatus?.Status switch
        {
            BuildStatus.Success => Brushes.DarkGreen,
            BuildStatus.Failure => Brushes.DarkRed,
            BuildStatus.InProgress => Brushes.DodgerBlue,
            BuildStatus.Unstable => Brushes.OrangeRed,
            BuildStatus.Stopped => Brushes.Gray,
            _ => null,
        };
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = revision.BuildStatus?.Tooltip ?? revision.BuildStatus?.Description;
        return toolTip is not null;
    }
}
