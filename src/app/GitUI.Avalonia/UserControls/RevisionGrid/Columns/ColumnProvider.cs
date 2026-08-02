using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

/// <summary>
/// Base class for columns shown in the revisions grid control.
/// </summary>
internal abstract class ColumnProvider
{
    protected ColumnProvider(string name, GridLength width, double minimumWidth, bool resizable)
    {
        Name = name;
        Column = new RevisionGridColumn(name, width, minimumWidth, resizable);
    }

    public double ColumnLeftMargin { get; } = 6;

    /// <summary>Gets the layout model for this column.</summary>
    public RevisionGridColumn Column { get; }

    /// <summary>Gets the display-friendly name of this column.</summary>
    public string Name { get; }

    public int Index { get; internal set; }

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
    public abstract void UpdateCell(Control control, GitRevision revision);

    /// <summary>Attempts to get custom tooltip text for this column.</summary>
    public virtual bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
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

    /// <summary>
    /// Gets or sets whether the column's owning integration can currently provide real content.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    public GridLength EffectiveWidth => IsVisible && IsAvailable ? Width : new GridLength(0);
}
