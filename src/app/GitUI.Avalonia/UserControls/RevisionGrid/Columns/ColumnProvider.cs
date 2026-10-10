using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

/// <summary>
/// Base class for columns shown in the revisions grid control.
/// </summary>
internal abstract class ColumnProvider
{
    public int ColumnLeftMargin { get; } = 6;

    /// <summary>Gets the layout model for this column.</summary>
    public RevisionGridColumn Column { get; }

    /// <summary>The display friendly name of this column.</summary>
    public string Name { get; }

    public int Index { get; internal set; }
    protected ColumnProvider(
        string name,
        GridLength width,
        double minimumWidth,
        bool resizable,
        string? headerText = null)
    {
        Name = name;
        Column = new RevisionGridColumn(headerText ?? name, width, minimumWidth, resizable);
    }

    public virtual void ApplySettings()
    {
        Column.IsVisible = true;
    }

    public virtual void Clear()
    {
    }

    /// <summary>Creates this column's control for one recycled row.</summary>
    public abstract Control CreateCell();

    /// <summary>Updates this column's control for the supplied revision.</summary>
    public void UpdateCell(Control control, GitRevision revision)
    {
        OnCellFormatting(control, revision);
        OnCellPainting(control, revision);
        UpdateToolTip(control, revision);
    }

    /// <summary>Renders the content of a cell in this column.</summary>
    public abstract void OnCellPainting(Control control, GitRevision revision);

    /// <summary>Formats the textual representation of a cell in this column.</summary>
    /// <remarks>Implementations update the native text control before the retained-mode paint pass.</remarks>
    public virtual void OnCellFormatting(Control control, GitRevision revision)
    {
    }

    public virtual void OnColumnWidthChanged()
    {
    }

    /// <summary>Attempts to get custom tooltip text for this column.</summary>
    public virtual bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = null;
        return false;
    }

    /// <summary>Attempts to get custom tooltip text for a highlight in a cell in this column.</summary>
    /// <remarks>Returning <see langword="false"/> does not suppress the tooltip for truncated text.</remarks>
    public virtual bool TryGetToolTip(
        GitRevision revision,
        IGitRef? highlightRef,
        [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (AppSettings.ShowRevisionGridTooltips.Value)
        {
            return TryGetToolTip(revision, out toolTip);
        }

        toolTip = null;
        return false;
    }

    protected static TextBlock CreateTextBlock(double leftMargin = 0, double opacity = 1)
        => new()
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(leftMargin, 0, 2, 0),
            Opacity = opacity,
        };

    protected void UpdateToolTip(Control control, GitRevision revision)
    {
        ToolTip.SetTip(
            control,
            AppSettings.ShowRevisionGridTooltips.Value
                && TryGetToolTip(revision, out string? toolTip)
                    ? toolTip
                    : null);
    }
}

/// <summary>
/// Describes layout and visibility without coupling providers to DataGridView.
/// </summary>
internal sealed class RevisionGridColumn
{
    public RevisionGridColumn(string headerText, GridLength width, double minimumWidth, bool resizable)
    {
        HeaderText = headerText;
        Width = width;
        MinimumWidth = minimumWidth;
        Resizable = resizable;
    }

    public string HeaderText { get; }

    public GridLength Width { get; set; }

    public double MinimumWidth { get; }

    public bool Resizable { get; set; }

    public bool IsVisible { get; set; } = true;

    public GridLength EffectiveWidth => IsVisible ? Width : new GridLength(0);
}
