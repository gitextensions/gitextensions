using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;

using GitUI.Editor.Diff;

namespace GitUI.Editor;

/// <summary>
/// Renders a unified diff as two aligned panes ("old" on the left, "new" on the right),
/// like the side-by-side view in VS Code. Hosted inside <see cref="FileViewer"/> and shown
/// only for patch diffs when <see cref="AppSettings.SideBySideDiff"/> is enabled.
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
    }

    public Control Control => _container;

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
    public bool TryShow(string patchText, bool openLineNumbers)
    {
        Diff.SideBySideSplitter.SplitResult? split = SideBySideSplitter.TrySplit(patchText);
        if (split is null)
        {
            Hide();
            return false;
        }

        string leftText = RenderColumn(split.Left, openLineNumbers);
        string rightText = RenderColumn(split.Right, openLineNumbers);

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

        _left.SetText(leftText, null, ViewMode.Text, useGitColoring: false, contentIdentification: null);
        _right.SetText(rightText, null, ViewMode.Text, useGitColoring: false, contentIdentification: null);
        _container.SplitterMoved += _splitterMovedHandler;
        return true;
    }

    private static string RenderColumn(List<Diff.SideBySideSplitter.ColumnLine> lines, bool openLineNumbers)
    {
        const int numWidth = 5;
        System.Text.StringBuilder sb = new();
        foreach (Diff.SideBySideSplitter.ColumnLine line in lines)
        {
            if (openLineNumbers)
            {
                sb.Append(line.LineNumber > 0
                    ? $"{line.LineNumber.ToString().PadLeft(numWidth)} "
                    : new string(' ', numWidth + 1));
            }

            sb.AppendLine(line.Text);
        }

        return sb.ToString();
    }

    public void Hide()
    {
        _container.Visible = false;
    }

    public void Dispose()
    {
        _container.Dispose();
        _left.Dispose();
        _right.Dispose();
        _splitterMovedHandler = null;
    }
}
