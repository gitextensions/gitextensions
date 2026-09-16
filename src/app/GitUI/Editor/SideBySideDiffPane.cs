using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor;

/// <summary>
/// Renders a unified diff as two aligned panes ("old" on the left, "new" on the right),
/// like the side-by-side view in VS Code. Hosted inside <see cref="FileViewer"/> and shown
/// only for patch diffs when <see cref="AppSettings.SideBySideDiff"/> is enabled.
/// Changed lines get a colored background; for paired changed lines only the parts which
/// actually differ are highlighted, the identical prefix/suffix is dimmed.
/// </summary>
public sealed class SideBySideDiffPane : IDisposable
{
    private readonly FileViewerInternal _left;
    private readonly FileViewerInternal _right;
    private readonly SplitContainer _container;
    private readonly FileViewer _owner;
    private SplitterEventHandler? _splitterMovedHandler;
    private bool _syncingScroll;
    private bool _addedToOwner;

    private static readonly Color AddedBackColor = AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
    private static readonly Color RemovedBackColor = AppColor.AnsiTerminalRedBackNormal.GetThemeColor();

    public SideBySideDiffPane(FileViewer owner)
    {
        _owner = owner;

        _splitterMovedHandler = (_, _) => StoreSplitterDistance();

        _container = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Name = "sideBySideContainer",
            TabIndex = 1,
        };

        _left = new FileViewerInternal { Dock = DockStyle.Fill, Margin = new Padding(0), Name = "sideBySideLeft" };
        _right = new FileViewerInternal { Dock = DockStyle.Fill, Margin = new Padding(0), Name = "sideBySideRight" };

        _container.Panel1.Controls.Add(_left);
        _container.Panel2.Controls.Add(_right);
        _container.SplitterMoved += _splitterMovedHandler;

        // keep the two panes scrolled in lock-step
        _left.VScrollPositionChanged += (_, _) => SyncScroll(fromRight: false);
        _right.VScrollPositionChanged += (_, _) => SyncScroll(fromRight: true);

        // forward hover events so the host can show/hide its floating toolbar
        _left.MouseMove += (sender, e) => MouseMove?.Invoke(sender, e);
        _right.MouseMove += (sender, e) => MouseMove?.Invoke(sender, e);
        _left.MouseLeave += (sender, e) => MouseLeave?.Invoke(sender, e);
        _right.MouseLeave += (sender, e) => MouseLeave?.Invoke(sender, e);
    }

    public Control Control => _container;

    /// <summary>
    /// Raised when the mouse moves over either pane (needed so the host can show its toolbar).
    /// </summary>
    public event MouseEventHandler? MouseMove;

    /// <summary>
    /// Raised when the mouse leaves either pane (needed so the host can hide its toolbar).
    /// </summary>
    public event EventHandler? MouseLeave;

    public bool Visible => _container.Visible;

    private void StoreSplitterDistance()
    {
        if (_container.Width > 0 && !_container.IsDisposed)
        {
            AppSettings.SideBySideDiffSplitPerMille = (int)Math.Round(1000.0 * _container.SplitterDistance / _container.Width);
        }
    }

    private void SyncScroll(bool fromRight)
    {
        if (_syncingScroll)
        {
            return;
        }

        _syncingScroll = true;
        try
        {
            FileViewerInternal source = fromRight ? _right : _left;
            FileViewerInternal target = fromRight ? _left : _right;
            target.VScrollPosition = source.VScrollPosition;
        }
        finally
        {
            _syncingScroll = false;
        }
    }

    /// <summary>
    /// Try to present the given text as a side-by-side diff. Returns false when the text
    /// is not a unified patch (caller then keeps showing it in the normal viewer).
    /// </summary>
    public bool TryShow(string patchText, bool openLineNumbers, string? fileName)
    {
        Diff.SideBySideSplitter.SplitResult? split = SideBySideSplitter.TrySplit(patchText);
        if (split is null)
        {
            Hide();
            return false;
        }

        string leftText = RenderColumn(split.Left, openLineNumbers, out List<LineSegmentInfo> leftSegments);
        string rightText = RenderColumn(split.Right, openLineNumbers, out List<LineSegmentInfo> rightSegments);

        if (!_addedToOwner)
        {
            _owner.Controls.Add(_container);
            _container.BringToFront();
            _addedToOwner = true;
        }

        _container.Visible = true;

        // avoid storing the distance while we are restoring it
        _container.SplitterMoved -= _splitterMovedHandler;
        double ratio = Math.Clamp(AppSettings.SideBySideDiffSplitPerMille / 1000.0, 0.15, 0.85);
        if (_container.Width > 100)
        {
            _container.SplitterDistance = (int)(_container.Width * ratio);
        }

        // match the unified viewer: fixed-width font from settings (TextEditor defaults to Courier New otherwise)
        Font font = AppSettings.FixedWidthFont;
        _left.Font = font;
        _right.Font = font;

        _left.SetText(leftText, null, ViewMode.Text, useGitColoring: false, contentIdentification: null);
        _right.SetText(rightText, null, ViewMode.Text, useGitColoring: false, contentIdentification: null);
        _container.SplitterMoved += _splitterMovedHandler;

        if (AppSettings.ShowSyntaxHighlightingInDiff.Value && !string.IsNullOrEmpty(fileName))
        {
            _left.SetHighlightingForFile(fileName);
            _right.SetHighlightingForFile(fileName);
        }
        else
        {
            _left.SetHighlighting("");
            _right.SetHighlighting("");
        }

        ApplyHighlighting(_left, split.Left, leftSegments, split.Right);
        ApplyHighlighting(_right, split.Right, rightSegments, split.Left);
        return true;
    }

    /// <summary>
    /// Apply the "show nonprinting characters" state of the unified viewer to both panes.
    /// </summary>
    public void SetNonPrintingChars(bool show)
    {
        ICSharpCode.TextEditor.Document.EolMarkerStyle style = show
            ? (AppSettings.ShowEolMarkerAsGlyph
                ? ICSharpCode.TextEditor.Document.EolMarkerStyle.Glyph
                : ICSharpCode.TextEditor.Document.EolMarkerStyle.Text)
            : ICSharpCode.TextEditor.Document.EolMarkerStyle.None;
        _left.EolMarkerStyle = style;
        _right.EolMarkerStyle = style;
        _left.ShowSpaces = show;
        _left.ShowTabs = show;
        _right.ShowSpaces = show;
        _right.ShowTabs = show;
    }

    /// <summary>
    /// One row of the rendered column text: the document offset of the line, its content
    /// length (without the trailing newline) and the <see cref="Diff.SideBySideSplitter.ColumnLine"/> it belongs to.
    /// </summary>
    private sealed class LineSegmentInfo
    {
        public int Offset;
        public int Length;
        public Diff.SideBySideSplitter.ColumnLine Line = new();
    }

    /// <summary>
    /// Build the column text and remember where every line ended up in the produced string.
    /// </summary>
    private static string RenderColumn(List<Diff.SideBySideSplitter.ColumnLine> lines, bool openLineNumbers, out List<LineSegmentInfo> segments)
    {
        const int numWidth = 5;
        System.Text.StringBuilder sb = new();
        segments = [];
        foreach (Diff.SideBySideSplitter.ColumnLine line in lines)
        {
            int lineStart = sb.Length;
            if (openLineNumbers)
            {
                sb.Append(line.LineNumber > 0
                    ? $"{line.LineNumber.ToString().PadLeft(numWidth)} "
                    : new string(' ', numWidth + 1));
            }

            int contentStart = sb.Length;
            sb.AppendLine(line.Text);

            segments.Add(new LineSegmentInfo
            {
                Offset = lineStart,
                Length = (contentStart - lineStart) + line.Text.Length,
                Line = line,
            });
        }

        return sb.ToString();
    }

    /// <summary>
    /// Color one pane: full-line background for added/removed lines; when a removed and an
    /// added line are paired (same row in the opposite column), the identical prefix/suffix
    /// of both is dimmed so only the actually changed part stands out.
    /// </summary>
    private static void ApplyHighlighting(FileViewerInternal pane, List<Diff.SideBySideSplitter.ColumnLine> lines,
        List<LineSegmentInfo> segments, List<Diff.SideBySideSplitter.ColumnLine> oppositeColumn)
    {
        ICSharpCode.TextEditor.TextEditorControl editor = pane.GetTestAccessor().TextEditor;
        MarkerStrategy markerStrategy = editor.Document.MarkerStrategy;
        markerStrategy.RemoveAll(_ => true);

        List<TextMarker> markers = [];
        for (int index = 0; index < lines.Count; index++)
        {
            LineSegmentInfo segment = segments[index];
            Diff.SideBySideSplitter.ColumnLine line = lines[index];

            if (line.Type is not (DiffLineType.Minus or DiffLineType.Plus) || line.Text.Length == 0)
            {
                continue;
            }

            Color backColor = line.Type == DiffLineType.Minus ? RemovedBackColor : AddedBackColor;
            markers.Add(CreateLineMarker(segment.Offset, segment.Length, backColor));

            // a paired row always has the opposite change type on the other side at the same row
            Diff.SideBySideSplitter.ColumnLine? opposite = index < oppositeColumn.Count
                ? oppositeColumn[index]
                : null;
            if (opposite is null)
            {
                continue;
            }

            bool oppositeIsPaired = line.Type switch
            {
                DiffLineType.Minus => opposite.Type == DiffLineType.Plus,
                DiffLineType.Plus => opposite.Type == DiffLineType.Minus,
                _ => false
            };

            if (!oppositeIsPaired || opposite.Text.Length == 0)
            {
                continue;
            }

            (int identicalAtStart, int identicalAtEnd) = GetIdenticalRange(line.Text, opposite.Text);
            int contentOffset = segment.Offset + (segment.Length - line.Text.Length);

            if (identicalAtStart > 0)
            {
                markers.Add(CreateDimmedMarker(contentOffset, identicalAtStart, backColor));
            }

            if (identicalAtEnd > 0)
            {
                markers.Add(CreateDimmedMarker(contentOffset + line.Text.Length - identicalAtEnd, identicalAtEnd, backColor));
            }
        }

        markerStrategy.AddMarkers(markers);
        editor.Refresh();
    }

    /// <summary>
    /// Common identical prefix and suffix length of the two texts (capped so they never overlap).
    /// </summary>
    private static (int IdenticalAtStart, int IdenticalAtEnd) GetIdenticalRange(string removed, string added)
    {
        int maxCommon = Math.Min(removed.Length, added.Length);
        int start = 0;
        while (start < maxCommon && removed[start] == added[start])
        {
            start++;
        }

        int end = 0;
        while (end < maxCommon - start && removed[removed.Length - 1 - end] == added[added.Length - 1 - end])
        {
            end++;
        }

        return (start, end);
    }

    /// <summary>
    /// Full-line background like the unified diff coloring.
    /// </summary>
    private static TextMarker CreateLineMarker(int offset, int length, Color backColor)
        => new(offset, Math.Max(1, length), TextMarkerType.SolidBlock, backColor, backColor.GetTextColor());

    /// <summary>
    /// Dimmed background for the parts of a changed line which are identical to the other side.
    /// </summary>
    private static TextMarker CreateDimmedMarker(int offset, int length, Color backColor)
        => new(offset, Math.Max(1, length), TextMarkerType.SolidBlock, backColor.DimColor().DimColor(), backColor.DimColor().DimColor().GetTextColor());

    public void Hide()
    {
        _container.Visible = false;
    }

    public void Dispose()
    {
        StoreSplitterDistance();
        _container.Dispose();
        _left.Dispose();
        _right.Dispose();
        _splitterMovedHandler = null;
    }
}
