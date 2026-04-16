using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private record struct Settings
        (bool FillRefLabels,
        bool NotesInSeparateColumn,
        bool ShowAnnotatedTagsMessages,
        bool ShowCommitBodyInRevisionGrid,
        bool ShowGitNotes,
        bool ShowGitStatusForArtificialCommits,
        bool ShowRemoteBranches,
        bool ShowTags);

    public const int MaxSuperprojectRefs = 4;

    private readonly StringBuilder _toolTipBuilder = new(200);

    private readonly Image _bisectGoodImage = DpiUtil.Scale(Images.BisectGood);
    private readonly Image _bisectBadImage = DpiUtil.Scale(Images.BisectBad);
    private readonly Image _fixupAndSquashImage = DpiUtil.Scale(Images.FixupAndSquashMessageMarker);

    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;
    private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

    /// <summary>
    ///  Caches painted ref label hit regions per row index for mouse hit-testing.
    /// </summary>
    private readonly Dictionary<int, List<RefLabelHitInfo>> _refLabelHitInfoByRow = [];

    // Pool of reusable lists to reduce allocations during scrolling.
    private readonly Stack<List<RefLabelHitInfo>> _hitInfoListPool = new();

    // The ref currently under the mouse cursor, used to draw a highlight border.
    private IGitRef? _highlightedRef;

    // The row index of the currently highlighted ref label.
    private int _highlightedRowIndex = -1;

    // The row index of the currently highlighted stash label (which has no IGitRef).
    private int _highlightedStashRow = -1;

    private Settings _settings;

    public MessageColumnProvider(RevisionGridControl grid, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder, ICommitDataManager? commitDataManager)
        : base("Message")
    {
        _commitDataManager = commitDataManager;
        _grid = grid;
        _gitRevisionSummaryBuilder = gitRevisionSummaryBuilder;

        Column = new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            HeaderText = "Message",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Width = DpiUtil.Scale(500),
            MinimumWidth = DpiUtil.Scale(25)
        };
    }

    public override void ApplySettings()
    {
        _settings = new Settings(
            FillRefLabels: AppSettings.FillRefLabels,
            NotesInSeparateColumn: AppSettings.ShowGitNotesColumn.Value,
            ShowAnnotatedTagsMessages: AppSettings.ShowAnnotatedTagsMessages,
            ShowCommitBodyInRevisionGrid: AppSettings.ShowCommitBodyInRevisionGrid,
            ShowGitNotes: AppSettings.ShowGitNotes,
            ShowGitStatusForArtificialCommits: AppSettings.ShowGitStatusForArtificialCommits,
            ShowRemoteBranches: AppSettings.ShowRemoteBranches,
            ShowTags: AppSettings.ShowTags);
    }

    public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
    {
        Validates.NotNull(e.Graphics);

        MultilineIndicator indicator = new(e, revision);
        Rectangle messageBounds = indicator.RemainingCellBounds;
        List<IGitRef> superprojectRefs = [];
        int offset = ColumnLeftMargin;

        List<RefLabelHitInfo>? hitInfos = null;

        if (_grid.TryGetSuperProjectInfo(out SuperProjectInfo? spi))
        {
            // Draw super project references (for submodules)
            DrawSuperprojectInfo(e, spi, revision, style, messageBounds, ref offset);

            if (spi.Refs is not null && revision.ObjectId is not null &&
                spi.Refs.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs))
            {
                superprojectRefs.AddRange(refs);
            }
        }

        if (revision.Refs.Count != 0)
        {
            IReadOnlyList<IGitRef> gitRefs = SortRefs(revision.Refs.Where(FilterRef));
            foreach (IGitRef gitRef in gitRefs)
            {
                if (offset > messageBounds.Width)
                {
                    // Stop drawing refs if we run out of room
                    break;
                }

                IGitRef? superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);
                if (superprojectRef is not null)
                {
                    superprojectRefs.Remove(superprojectRef);
                }

                bool isHighlighted = _highlightedRowIndex == e.RowIndex && ReferenceEquals(_highlightedRef, gitRef);
                Rectangle refRect = DrawRef(e, gitRef, superprojectRef, style, messageBounds, ref offset, isHighlighted);
                if (refRect != Rectangle.Empty)
                {
                    hitInfos ??= RentHitInfoList();
                    hitInfos.Add(new RefLabelHitInfo(refRect, gitRef, StashReflogSelector: null));
                }
            }
        }

        DrawSuperprojectRefs(e, superprojectRefs, style, messageBounds, ref offset);

        if (revision.IsStash || revision.IsAutostash)
        {
            bool isStashHighlighted = _highlightedRowIndex == e.RowIndex && _highlightedRef is null && _highlightedStashRow == e.RowIndex;
            Rectangle stashRect = RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref offset,
                revision.IsAutostash ? revision.Subject : (revision.ReflogSelector ?? throw new InvalidOperationException($"{nameof(revision.ReflogSelector)} must not be null"))[5..],
                AppColor.OtherTag.GetThemeColor(),
                RefArrowType.None,
                messageBounds,
                e.Graphics,
                dashedLine: false,
                fill: _settings.FillRefLabels,
                highlight: isStashHighlighted);
            if (stashRect != Rectangle.Empty)
            {
                hitInfos ??= RentHitInfoList();
                hitInfos.Add(new RefLabelHitInfo(stashRect, GitRef: null, StashReflogSelector: revision.ReflogSelector));
            }
        }

        if (revision.IsArtificial)
        {
            DrawArtificialRevision(e, revision, style, messageBounds, ref offset);
        }
        else if (!revision.IsAutostash)
        {
            DrawCommitMessage(e, revision, style, messageBounds, indicator, ref offset);
        }

        if (hitInfos is not null)
        {
            if (_refLabelHitInfoByRow.Remove(e.RowIndex, out List<RefLabelHitInfo>? oldList))
            {
                ReturnHitInfoList(oldList);
            }

            _refLabelHitInfoByRow[e.RowIndex] = hitInfos;
        }
        else if (_refLabelHitInfoByRow.Remove(e.RowIndex, out List<RefLabelHitInfo>? oldList))
        {
            ReturnHitInfoList(oldList);
        }
    }

    public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
    {
        // Set the grid cell's accessibility text
        e.Value = revision.Subject.Trim();
    }

    public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        _toolTipBuilder.Clear();

        if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
        {
            // The body is not stored for older commits (to save memory)
            string bodySummary = _gitRevisionSummaryBuilder.BuildSummary(GetBody(revision))
                ?? revision.Subject + (revision.HasMultiLineMessage ? TranslatedStrings.BodyNotLoaded : "");
            int initialLength = bodySummary.Length + 10;
            _toolTipBuilder.EnsureCapacity(initialLength);

            _toolTipBuilder.Append(bodySummary);

            if (revision.Refs.Count != 0)
            {
                if (_toolTipBuilder.Length != 0)
                {
                    _toolTipBuilder.AppendLine();
                    _toolTipBuilder.AppendLine();
                }

                foreach (IGitRef gitRef in SortRefs(revision.Refs))
                {
                    if (gitRef.IsBisectGood)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsGood);
                    }
                    else if (gitRef.IsBisectBad)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsBad);
                    }
                    else
                    {
                        _toolTipBuilder.Append('[').Append(gitRef.Name).Append(']').AppendLine();
                    }
                }
            }

            toolTip = _toolTipBuilder.ToString();
            return true;
        }

        if (_settings.ShowGitStatusForArtificialCommits && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            toolTip = _toolTipBuilder.Append(changeCount.GetSummary()).ToString();
            return true;
        }

        return base.TryGetToolTip(e, revision, out toolTip);
    }

    private void DrawArtificialRevision(
        DataGridViewCellPaintingEventArgs e,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        Graphics graphics = e.Graphics!; // validated by caller
        int baseOffset = offset;

        // Add fake "refs" for artificial commits
        RevisionGridRefRenderer.DrawRef(
            e.State.HasFlag(DataGridViewElementStates.Selected),
            style.NormalFont,
            ref offset,
            revision.Subject,
            AppColor.OtherTag.GetThemeColor(),
            RefArrowType.None,
            messageBounds,
            graphics,
            dashedLine: false,
            fill: _settings.FillRefLabels);

        int max = Math.Max(
            TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Workspace, style.NormalFont).Width,
            TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Index, style.NormalFont).Width);

        offset = baseOffset + max + DpiUtil.Scale(6);

        // Summary of changes
        if (!_settings.ShowGitStatusForArtificialCommits || _grid.GetChangeCount(revision.ObjectId) is not ArtificialCommitChangeCount changeCount)
        {
            return;
        }

        if (changeCount.DataValid)
        {
            if (changeCount.HasChanges)
            {
                DrawArtificialCount(changeCount.Changed, Images.FileStatusModified, ref offset);
                DrawArtificialCount(changeCount.New, Images.FileStatusAdded, ref offset);
                DrawArtificialCount(changeCount.Deleted, Images.FileStatusRemoved, ref offset);
                DrawArtificialCount(changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown, ref offset);
                DrawArtificialCount(changeCount.SubmodulesDirty, Images.SubmoduleDirty, ref offset);
            }
            else
            {
                DrawArtificialCount(items: null, Images.RepoStateClean, ref offset);
            }
        }
        else
        {
            DrawArtificialCount(items: null, Images.RepoStateUnknown, ref offset);
        }

        return;

        void DrawArtificialCount(
            IReadOnlyList<GitItemStatus>? items,
            Image icon,
            ref int offset)
        {
            if (items?.Count is 0)
            {
                return;
            }

            int imageVerticalPadding = DpiUtil.Scale(6);
            int textHorizontalPadding = DpiUtil.Scale(4);
            int imageSize = e.CellBounds.Height - imageVerticalPadding - imageVerticalPadding;
            Rectangle imageRect = new(
                messageBounds.Left + offset,
                e.CellBounds.Top + imageVerticalPadding,
                imageSize,
                imageSize);

            System.Drawing.Drawing2D.GraphicsContainer container = graphics.BeginContainer();
            graphics.DrawImage(icon, imageRect);
            graphics.EndContainer(container);
            offset += imageSize + textHorizontalPadding;

            string text = items?.Count.ToString() ?? "";
            Rectangle bounds = messageBounds.ReduceLeft(offset);
            int textWidth = Math.Max(
                _grid.DrawColumnText(e, text, style.NormalFont, style.ForeColor, bounds),
                TextRenderer.MeasureText("88", style.NormalFont).Width);
            offset += textWidth + textHorizontalPadding;
        }
    }

    private static void DrawSuperprojectRefs(
        DataGridViewCellPaintingEventArgs e,
        List<IGitRef> superprojectRefs,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        for (int i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
        {
            IGitRef gitRef = superprojectRefs[i];
            Color headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);
            string gitRefName = i < (MaxSuperprojectRefs - 1) ? gitRef.Name : "…";

            RefArrowType arrowType = gitRef.IsSelected
                ? RefArrowType.Filled
                : gitRef.IsSelectedHeadMergeSource
                    ? RefArrowType.NotFilled
                    : RefArrowType.None;
            Font font = gitRef.IsSelected ? style.BoldFont : style.NormalFont;

            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                font,
                ref offset,
                gitRefName,
                headColor,
                arrowType,
                messageBounds,
                e.Graphics!,
                dashedLine: true);
        }
    }

    private static void DrawSuperprojectInfo(
        DataGridViewCellPaintingEventArgs e,
        SuperProjectInfo spi,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        if (spi.CurrentCommit == revision.ObjectId)
        {
            DrawSuperProjectRef("", ref offset, isSelected: true);
        }

        if (spi.ConflictBase == revision.ObjectId)
        {
            DrawSuperProjectRef("Base", ref offset, isSelected: false);
        }

        if (spi.ConflictLocal == revision.ObjectId)
        {
            DrawSuperProjectRef("Local", ref offset, isSelected: false);
        }

        if (spi.ConflictRemote == revision.ObjectId)
        {
            DrawSuperProjectRef("Remote", ref offset, isSelected: false);
        }

        void DrawSuperProjectRef(string label, ref int currentOffset, bool isSelected)
        {
            // Rectangle does not have a BackColor property. Use the cell's background color instead.
            Color backColor = e.CellStyle?.BackColor ?? ThemeSettings.Default.Theme.GetColor(AppColor.EditorBackground);
            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref currentOffset,
                label,
                headColor: Color.OrangeRed.AdaptForeColor(backColor),
                isSelected ? RefArrowType.Filled : RefArrowType.NotFilled,
                messageBounds,
                e.Graphics!,
                dashedLine: true);
        }
    }

    private Rectangle DrawRef(
        DataGridViewCellPaintingEventArgs e,
        IGitRef gitRef,
        IGitRef? superprojectRef,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset,
        bool highlight)
    {
        if (gitRef.IsBisect)
        {
            if (gitRef.IsBisectGood)
            {
                DrawImage(e, _bisectGoodImage, messageBounds, ref offset);
                return Rectangle.Empty;
            }

            if (gitRef.IsBisectBad)
            {
                DrawImage(e, _bisectBadImage, messageBounds, ref offset);
                return Rectangle.Empty;
            }
        }

        if (!style.RemoteColors.TryGetValue(gitRef.Remote, out Color headColor))
        {
            headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);
        }

        RefArrowType arrowType = gitRef.IsSelected
            ? RefArrowType.Filled
            : gitRef.IsSelectedHeadMergeSource
                ? RefArrowType.NotFilled
                : RefArrowType.None;

        Font font = gitRef.IsSelected
            ? style.BoldFont
            : style.NormalFont;

        string name = gitRef.Name;

        if (gitRef.IsTag &&
            gitRef.IsDereference && // see note on using IsDereference in CommitInfo class
            _settings.ShowAnnotatedTagsMessages)
        {
            name += " [...]";
        }

        return RevisionGridRefRenderer.DrawRef(
            e.State.HasFlag(DataGridViewElementStates.Selected),
            font,
            ref offset,
            name,
            headColor,
            arrowType,
            messageBounds,
            e.Graphics!,
            dashedLine: superprojectRef is not null,
            fill: _settings.FillRefLabels,
            highlight: highlight);
    }

    private static void DrawImage(
        DataGridViewCellPaintingEventArgs e,
        Image image,
        Rectangle messageBounds,
        ref int offset)
    {
        int x = e.CellBounds.X + offset;
        if (x + image.Width < messageBounds.Right)
        {
            int y = e.CellBounds.Y + ((e.CellBounds.Height - image.Height) / 2);
            e.Graphics!.DrawImage(image, x, y);
            offset += image.Width + DpiUtil.Scale(4);
        }
    }

    private void DrawCommitMessage(
        DataGridViewCellPaintingEventArgs e,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        MultilineIndicator indicator,
        ref int offset)
    {
        string[] lines = GetCommitMessageLines(revision);
        if (lines.Length == 0)
        {
            return;
        }

        string commitTitle = lines[0];

        // Draw markers for fixup! and squash! commits
        if (commitTitle.StartsWith(CommitKind.Fixup.GetPrefix()) || commitTitle.StartsWith(CommitKind.Squash.GetPrefix()) || commitTitle.StartsWith(CommitKind.Amend.GetPrefix()))
        {
            DrawImage(e, _fixupAndSquashImage, messageBounds, ref offset);
        }

        if (offset > messageBounds.Width)
        {
            return;
        }

        Font font = revision.ObjectId == _grid.CurrentCheckout ? style.BoldFont : style.NormalFont;
        Rectangle titleBounds = messageBounds.ReduceLeft(offset);
        int titleWidth = _grid.DrawColumnText(e, commitTitle, font, style.ForeColor, titleBounds);
        offset += titleWidth;

        if (offset > messageBounds.Width)
        {
            return;
        }

        if (lines.Length > 1 && _settings.ShowCommitBodyInRevisionGrid)
        {
            string commitBody = string.Concat(lines.Skip(1).Select(_ => " " + _));
            Rectangle bodyBounds = messageBounds.ReduceLeft(offset);
            int bodyWidth = _grid.DrawColumnText(e, commitBody, font, style.CommitBodyForeColor, bodyBounds);
            offset += bodyWidth;
        }

        // Draw the multi-line indicator
        indicator.Render();
    }

    private bool FilterRef(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return _settings.ShowTags;
        }

        if (gitRef.IsRemote)
        {
            return _settings.ShowRemoteBranches;
        }

        return true;
    }

    private static IReadOnlyList<IGitRef> SortRefs(IEnumerable<IGitRef> refs)
    {
        List<IGitRef> sortedRefs = [.. refs];
        sortedRefs.Sort(CompareRefs);
        return sortedRefs;

        static int CompareRefs(IGitRef left, IGitRef right)
        {
            int leftTypeRank = RefTypeRank(left);
            int rightTypeRank = RefTypeRank(right);

            int c = leftTypeRank.CompareTo(rightTypeRank);

            return c == 0
                ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                : c;

            static int RefTypeRank(IGitRef gitRef)
            {
                if (gitRef.IsBisect)
                {
                    return 0;
                }

                if (gitRef.IsSelected)
                {
                    return 1;
                }

                if (gitRef.IsSelectedHeadMergeSource)
                {
                    return 2;
                }

                if (gitRef.IsHead)
                {
                    return 3;
                }

                if (gitRef.IsRemote)
                {
                    return 4;
                }

                return 5;
            }
        }
    }

    private string[] GetCommitMessageLines(GitRevision revision)
        => GetBody(revision)?.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries) ?? [revision.Subject];

    private string? GetBody(GitRevision revision)
    {
        if (revision.Body is null)
        {
            if (_commitDataManager is not null && (_settings.ShowCommitBodyInRevisionGrid || _settings.ShowGitNotes || _settings.NotesInSeparateColumn))
            {
                _commitDataManager.InitiateDelayedLoadingOfDetails(revision);
            }

            return null;
        }

        return _settings.NotesInSeparateColumn
            ? revision.Body
            : UIExtensions.FormatBodyAndNotes(revision.Body, revision.Notes);
    }

    /// <summary>
    ///  Performs a hit test to find which ref label (if any) contains the given point in the specified row.
    /// </summary>
    /// <returns>The matching <see cref="RefLabelHitInfo"/>, or <see langword="null"/> if no ref label was hit.</returns>
    public RefLabelHitInfo? HitTest(int rowIndex, Point gridClientPoint)
    {
        if (!_refLabelHitInfoByRow.TryGetValue(rowIndex, out List<RefLabelHitInfo>? hitInfos))
        {
            return null;
        }

        foreach (RefLabelHitInfo hitInfo in hitInfos)
        {
            if (hitInfo.Bounds.Contains(gridClientPoint))
            {
                return hitInfo;
            }
        }

        return null;
    }

    /// <summary>
    ///  Sets the ref or stash label to be drawn with a highlight border, triggering a repaint if the highlight changed.
    /// </summary>
    /// <returns><see langword="true"/> if the highlight state changed and a repaint is needed.</returns>
    public bool SetHighlight(int rowIndex, RefLabelHitInfo? hitInfo)
    {
        IGitRef? gitRef = hitInfo?.GitRef;
        int stashRow = hitInfo?.StashReflogSelector is not null ? rowIndex : -1;

        if (_highlightedRowIndex == rowIndex && ReferenceEquals(_highlightedRef, gitRef) && _highlightedStashRow == stashRow)
        {
            return false;
        }

        _highlightedRef = gitRef;
        _highlightedRowIndex = hitInfo is not null ? rowIndex : -1;
        _highlightedStashRow = stashRow;
        return true;
    }

    public override void Clear()
    {
        foreach (List<RefLabelHitInfo> list in _refLabelHitInfoByRow.Values)
        {
            ReturnHitInfoList(list);
        }

        _refLabelHitInfoByRow.Clear();
        _highlightedRef = null;
        _highlightedRowIndex = -1;
        _highlightedStashRow = -1;
    }

    private List<RefLabelHitInfo> RentHitInfoList()
    {
        return _hitInfoListPool.TryPop(out List<RefLabelHitInfo>? list) ? list : [];
    }

    private void ReturnHitInfoList(List<RefLabelHitInfo> list)
    {
        list.Clear();
        _hitInfoListPool.Push(list);
    }
}
